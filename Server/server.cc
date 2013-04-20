#include <iostream>
#include <fstream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include <sstream>
#include <cstdlib>
#include <algorithm>
#include <iterator>
#include <map>
#include <set>
#include <vector>
#include <deque>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <boost/thread.hpp>
#include <boost/date_time.hpp>
#include "spreadsheet.h"

using namespace std;

//to facilitate working with what commands are
//set from the client to the server and the methods
//we need to call on them
enum com_cmds{
	CREATE,
	JOIN,
	CHANGE,
	UNDO,
	UPDATE,
	SAVE,
	LEAVE,
	ERROR = -1
};

//global hash map of spreadsheets and connections
map<string, list<int> > ss_connections;

// list of open/connected  spreadsheets
vector<spreadsheet> connected_ss;
// prints out an error message
void error(const char *msg)
{
	perror(msg);
	exit(1);
}


//=======================parser

void updateCommand(string update, int connection, string SSname)
{
	update.erase(0, 6);
	update.insert(0, "UPDATE");
	// LOOP THROUGH CONNECTIONS AND SEND IT TO ALL OTHER connections except the one that sent change
	// Initialize iterator
	std::map <std::string, int>::iterator it;
	// Initialize buffer for writing to socket
	char buffer[update.length()];
	// Copy string to send into char array
	std::size_t length = update.copy(buffer, 0, update.length());
	buffer[length] = '\0';
	// Initialize temp list to loop through all connections for given SS name
	list<int> temp = ss_connections.find(SSname)->second;
	for (int i = 0; i < temp.size(); i++)
	{
		int tempConnection = temp.front();
		// Check to make sure the connection in the list is not the connection that sent the CHANGE request
		// Send all other connections the update message
		if (connection != tempConnection)
		{
			write(connection, buffer, update.length());
		}
		temp.pop_front();
		temp.push_back(tempConnection);
	}
}

string changeCommand(string change, int connection)
{
	vector<string> info;
	stringstream ss(change);
	string item;
	string serverResponse = "";
	while(getline(ss, item))
	{
		info.push_back(item);
	}

	std::cout << info[1] << "\n" << info[2] << std::endl;
	stringstream tempSS(info[2]);
	vector<string> versionInfo;
	string temp1;
	string tempName;

	while(getline(tempSS, temp1, ':' ))
	{
		versionInfo.push_back(temp1);
	}
	unsigned pos = temp1.find(" ");
	temp1 = temp1.substr(0, pos);
	int testVersion = atoi(temp1.c_str());

	stringstream nameStream(info[3]);
	vector<string> cellNameInfo;
	string temp2;
	string tempcellName;

	while(getline(tempSS, temp2, ':'))
	{
		cellNameInfo.push_back(temp2);
	}
	unsigned pos2 = temp2.find(" ");
	temp2 = temp2.substr(0, pos);
	bool testVersionEqualsSpreadsheetVersion = true;

	if(testVersionEqualsSpreadsheetVersion)
	{
		// increment spreadsheet version
		int SSversion = testVersion +1;
		stringstream serverResponseSS;
		serverResponseSS << "CHANGE SP OK \n";
		serverResponseSS << info[1];
		serverResponseSS << " \n";
		serverResponseSS << "Version:";
		serverResponseSS << SSversion;
		serverResponseSS << " \n";
		serverResponse = serverResponseSS.str();
		// Send update to spreadsheet
		for (int i = 0; i < connected_ss.size(); i++)
		{
			if (tempName == connected_ss[i].get_name())
			{
				//connected_ss.edit_cell_content(
			}
		}
		// Change request is valid, call updateCommand to send out the update
		updateCommand(change, connection, tempName);
	}
	else if(!testVersionEqualsSpreadsheetVersion)
	{
		stringstream serverResponseSS;
		int SSversion = testVersion;
		serverResponseSS << "CHANGE WAIT OK \n";
		serverResponseSS << info[1];
		serverResponseSS << " \n";
		serverResponseSS << "Version:";
		serverResponseSS << SSversion;
		serverResponseSS << " \n";
		serverResponse = serverResponseSS.str();
		std::cout << serverResponse << std::endl;
	}
	else
	{
		stringstream serverResponseSS;
		int SSversion = testVersion;
		serverResponseSS << "CHANGE FAIL \n";
		serverResponseSS << info[1];
		serverResponseSS << "\n";
		serverResponseSS << "MESSAGE REGARDING FAIL\n";
		serverResponse = serverResponseSS.str();
		std::cout << serverResponse << std::endl;
	}


	return serverResponse;

}

string undoCommand()
{

}

string createCommand(string create, int connection)
{

	vector<string> info;
	stringstream ss(create);
	string serverResponse = "";
	string item;
	while(getline(ss, item))
	{
		info.push_back(item);
	}

	std::cout << info[1] << "\n" << info[2] << std::endl;
	stringstream tempSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	while(getline(tempSS, tempName, ':' ))
	{
		nameInfo.push_back(tempName);
	}
	unsigned pos = tempName.find(" ");
	tempName = tempName.substr(0, pos);

	// Add name and Connection to the map
	list<int> connList;
	ss_connections.insert ( std::pair<std::string, list<int> >(tempName, connList));
	ss_connections.at(tempName).push_front(connection);

	stringstream tempSS2(info[2]);
	vector<string> passwordInfo;
	string tempPassword;
	while(getline(tempSS2, tempPassword, ':' ))
	{
		nameInfo.push_back(tempPassword);
	}
	pos = tempPassword.find(" ");
	tempPassword = tempPassword.substr(0, pos);
	std::cout << "tempName is: " << tempName << std::endl << "tempPassword is: " << tempPassword << std::endl << std::endl;
	bool testNameNotTaken = true; // Test if file name exists already


	// Add spreadsheet to list of existing spreadsheets
	spreadsheet::spreadsheet newSS(tempName, tempPassword, 0);
	connected_ss.push_back(newSS);

	if(testNameNotTaken) // Name is not taken
	{
		// Create spreadsheet with name and password (Use hashmaps to keep track of spreadsheets?)    

		stringstream serverResponseSS;
		serverResponseSS << "CREATE SP OK \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";
		serverResponseSS << "Password:";
		serverResponseSS << tempPassword;
		serverResponseSS << " \n";

		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}
	else
	{
		stringstream serverResponseSS;
		serverResponseSS << "CREATE SP FAIL \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";
		serverResponseSS << "MESSAGE REGARDING FAIL";
		serverResponseSS << " \n";

		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}

	return serverResponse;
}

string joinCommand(string join, int connection)
{

	vector<string> info;
	stringstream ss(join);
	string serverResponse = "";
	string item;
	while(getline(ss, item))
	{
		info.push_back(item);
	}

	std::cout << info[1] << "\n" << info[2] << std::endl;

	stringstream tempSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	while(getline(tempSS, tempName, ':' ))
	{
		nameInfo.push_back(tempName);
	}
	unsigned pos = tempName.find(" ");
	tempName = tempName.substr(0, pos);

	// Add name and Connection to the map
	list<int> connList;
	ss_connections.insert ( std::pair<std::string, list<int> >(tempName, connList));
	ss_connections.at(tempName).push_front(connection);

	stringstream tempSS2(info[2]);
	vector<string> passwordInfo;
	string tempPassword;
	while(getline(tempSS2, tempPassword, ':' ))
	{
		nameInfo.push_back(tempPassword);
	}
	pos = tempPassword.find(" ");
	tempPassword = tempPassword.substr(0, pos);
	std::cout << "tempName is: " << tempName << std::endl << "tempPassword is: " << tempPassword << std::endl << std::endl;

	bool nameExists = true; // Check to see if name exists
	bool passwordMatches = true; // Check if password matches

	if(nameExists && passwordMatches)
	{

		// Retrieve spreadsheet information ----- Need to implement -------------
		int SSversion = 0; // Get current version number of spreadsheet
		int lengthOfSpreadsheetXML = 1313; // lengthOfSpreadsheetXML = SpreadsheetXML.length();
		std::string xml = "TESTXML"; // xml = readtextfile(tempName);
		stringstream serverResponseSS;
		serverResponseSS << "JOIN SP OK \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";
		serverResponseSS << "Version:";
		serverResponseSS << SSversion;
		serverResponseSS << " \n";
		serverResponseSS << "Length:";
		serverResponseSS << lengthOfSpreadsheetXML;
		serverResponseSS << "\n";
		serverResponseSS << xml;
		serverResponseSS << "\n";


		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}
	else
	{
		stringstream serverResponseSS;
		serverResponseSS << "JOIN SP FAIL \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";
		serverResponseSS << "MESSAGE REGARDING FAIL";
		serverResponseSS << " \n";

		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}



	return serverResponse;
}

string saveCommand(string save)
{

	vector<string> info;
	stringstream ss(save);
	string serverResponse = "";
	string item;
	while(getline(ss, item))
	{
		info.push_back(item);
	}

	std::cout << info[1] << "\n" << info[2] << std::endl;
	stringstream tempSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	while(getline(tempSS, tempName, ':' ))
	{
		nameInfo.push_back(tempName);
	}
	unsigned pos = tempName.find(" ");
	tempName = tempName.substr(0, pos);

	bool testNameNotTaken = true; // Test if file name exists already


	if(testNameNotTaken) // Name is not taken
	{
		// Save spreadsheet with name and password (Use hashmaps to keep track of spreadsheets?)    

		stringstream serverResponseSS;
		serverResponseSS << "CREATE SP OK \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";

		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}
	else
	{
		stringstream serverResponseSS;
		serverResponseSS << "CREATE SP FAIL \n";
		serverResponseSS << "Name:";
		serverResponseSS << tempName;
		serverResponseSS << " \n";
		serverResponseSS << "MESSAGE REGARDING FAIL";
		serverResponseSS << " \n";

		serverResponse = serverResponseSS.str();

		std::cout << serverResponse << std::endl;
	}

	return serverResponse;
}
void leaveCommand()
{

}
int parse(char buf[256])
{
	if(buf[0] == 'C')
		if(buf[1] == 'R')
			return CREATE;
		else
			return CHANGE;

	if(buf[0] == 'J')
		return JOIN;

	if(buf[0] == 'U')
		if(buf[1] == 'N')
			return UNDO;
		else
			return UPDATE;

	if(buf[0] == 'S')
		return SAVE;

	if(buf[0] == 'L')
		return LEAVE;

	return ERROR;
}


//Creates a connection object that runs on it's own thread
//this class will have all the functionality that each 
//connection that is made will need
class Connection 
{
	public:
		//constructor
		//takes in the socket file descriptor for the connection
		Connection(int c_newsockfd, int num) 
			: newsockfd(c_newsockfd),
			con_num(num)
	{
		//in here we can put the information for the connection 
		//in a map/list to keep track of live connections
		//printf("%d\n", con_num);

	}

		//this method is necessary for multithreading. Starts listening
		//on the socket for messages to come through
		void operator()()
		{
			start_read();
		}

		int get_sockfd()
		{
			return newsockfd;
		}

		int get_con_num()
		{
			return con_num;
		}

	private:
		int n, newsockfd, con_num;
		char buffer[256];

		//listen on the socket connection for a message to come through
		void start_read()
		{
			//bzero clears out the buffer
			bzero(buffer, 256);
			//read will read the message off the socket connection and
			//store the message in buffer
			n = read(newsockfd, buffer, 255);
			if(n==0)
			{
				close_con();
				return;
			}
			if(n<0) error("ERROR reading from socket");
			//printf("%s",buffer);

			//here is where we need to parse the message to determine what
			//command has been sent by the client

			int cmd = parse(buffer);

			string message = string(buffer);
			string serv_resp = "ERROR \n";
			switch(cmd)
			{
				case CREATE: serv_resp = createCommand(message, newsockfd);
										 break;
				case JOIN: serv_resp = joinCommand(message, newsockfd);
									 break;
				case CHANGE: serv_resp = changeCommand(message, newsockfd);
										 break;
				case UNDO: serv_resp = undoCommand();
									 break;
				case SAVE: ; serv_resp = saveCommand(message);
									 break;
				case LEAVE: ;
										break;
				case ERROR: "";
										break;

			}

			char rspns[256]; 
			size_t length = serv_resp.copy(rspns,256, 0);
			rspns[length] = '\0';
			//here is where we will write back to the client the correct msg
			//we can either keep it here and pass back from the methods we call
			//what needs to be sent to the client or just send the message from
			//the method called and remove this call to start_write()
			n = start_write(rspns);
			if(n==0)
			{
				return;
			}
			//clear out the buffer (idk if we need it but it was causing some
			//issues when i wasn't
			bzero(buffer,256);
			//start listening on the socket connection again
			start_read();
		}

		//write the message to the client on the socket connection
		int start_write(char response[])
		{

			n = write(newsockfd, response, 100);
			if(n==0)
			{
				close_con();
				return 0;
			}
			if(n<0) error("Error writing to socket");
		}

		//gracefully close the connection when either the client sends a 
		//message to disconnect or the client side just decides to close
		void close_con()
		{
			close(newsockfd);
		}
};

int main(int argc, char* argv[])
{
	std::cout << "Jocadoet Server Running"<< std::endl;
	int sockfd, newsockfd, portno;
	socklen_t clilen;
	struct sockaddr_in serv_addr, cli_addr;
	//opens a socket
	sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (sockfd < 0)
		error("ERROR opening socket");
	bzero((char *) &serv_addr, sizeof(serv_addr));
	//set the port number
	portno = 1984;
	//set up the server info
	serv_addr.sin_family = AF_INET;
	serv_addr.sin_addr.s_addr = INADDR_ANY;
	serv_addr.sin_port = htons(portno);
	//bind the socket
	if (bind(sockfd, (struct sockaddr *) &serv_addr,
				sizeof(serv_addr)) < 0)
		error("ERROR on binding");
	//loop here and conitnually listen for connections
	//the 5 in the parameters is the amount of connection
	//requests that can be in queue at one time if there for
	//some reason is a problem with the socket accepting the client
	//connection
	int con_num = 0;
	while(1)
	{
		listen(sockfd, 5);

		clilen = sizeof(cli_addr);
		newsockfd = accept(sockfd,
				(struct sockaddr *) &cli_addr,
				&clilen);
		if(newsockfd < 0)
			error("ERROR on accept");

		//make a new connection object
		Connection c(newsockfd, con_num);
		//send it to it's own thread
		boost::thread conThread(c);

		//increment connection count
		con_num++;

	}

	//the close on the newsockfd shouldn't need to be done here but
	//inside the connection.(however i'm not sure if the actual
	//new socket file descriptor is sent and this one will need to
	//be closed or if it will be taken care of when we close it in the
	//connection object
	//close(newsockfd);	

	//close the server socket
	close(sockfd);

	return 0;
}
