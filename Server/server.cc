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
	cout << "need to catch exception" << endl;
	exit(1);
}

//================================================================updateCommand
void updateCommand(string update, int connection, string SSname)
{
	int n;
	update.erase(0, 6);
	update.insert(0, "UPDATE");
	// LOOP THROUGH CONNECTIONS AND SEND IT TO ALL OTHER connections except the 
	// one that sent change
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
		// Check to make sure the connection in the list is not the connection 
		// that sent the CHANGE request
		// Send all other connections the update message
		if (connection != tempConnection)
		{
			n = write(connection, buffer, update.length());
			if(n==0)
			{
				close(connection);
				return;
			}
			if(n < 0)
			{
				error("Error writing to socket");
			}
		}
		temp.pop_front();
		temp.push_back(tempConnection);
	}
}

//================================================================changeCommand
string changeCommand(string change, int connection)
{

	cout << change << "that's what it was billy\n" << endl;
	vector<string> info;
	stringstream ss(change);
	string item;
	string serverResponse = "";
	while(getline(ss, item))
	{
		info.push_back(item);
	}

	string tempName;
	vector<string> versionInfo;
	stringstream tempNameSS(info[1]);
	while(getline(tempNameSS, tempName, ':' ))
	{
		versionInfo.push_back(tempName);
	}
	unsigned posName = tempName.find(" ");
	tempName = tempName.substr(0, posName);
	
	stringstream tempSS(info[2]);
	string version;

	while(getline(tempSS, version, ':' ))
	{
		versionInfo.push_back(version);
	}
	unsigned pos = version.find(" ");
	version= version.substr(0, pos);
	int testVersion = atoi(version.c_str());

	stringstream nameStream(info[3]);
	vector<string> cellNameInfo;
	string cellName;
	while(getline(nameStream, cellName, ':'))
	{
		cellNameInfo.push_back(cellName);
	}
	unsigned pos2 = cellName.find(" ");
	cellName= cellName.substr(0, pos2);
	
	string cellContent = info[5];

	cout << tempName << "\n" <<  version << "\n" <<  cellName << "\n" <<  cellContent << endl;

	bool versionMatch = false;

	for(int i = 0; i < connected_ss.size(); i++)
	{
		if(tempName==connected_ss[i].get_name())
		{
			if(connected_ss[i].check_version(testVersion))
			{
				versionMatch = true;
			}
		}
	}

	if(versionMatch)
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
				connected_ss[i].edit_cell_content(version, cellName);
			}
		}
		// Change request is valid, call updateCommand to send out the update
		updateCommand(change, connection, tempName);
	}
	else if(!versionMatch)
	{
		stringstream serverResponseSS;
		int SSversion = testVersion;
		serverResponseSS << "CHANGE SP WAIT \n";
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

//================================================================undoCommand
string undoCommand()
{

}

//================================================================createCommand
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
	cout<< "inserting into map" << endl;
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

//================================================================joinCommand
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
	cout<< "inserting int map join" << endl;
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
		int lengthOfSpreadsheetXML = 1313; //lengthOfSpreadsheetXML=SpreadsheetXML.length();
		std::string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"ps6\"></spreadsheet>"; // xml = readtextfile(tempName);
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

//================================================================saveCommand
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
	cout << "herehere" << endl;
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

//================================================================leaveCommand
void leaveCommand()
{

}

//================================================================parse
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


//================================================================updateCommand
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
		printf("New connection made: %d\n", con_num);

	}

		~Connection()
		{
			//printf("destroyed connection: %d\n", con_num);
		}

		//this method is necessary for multithreading. Starts listening
		//on the socket for messages to come through
		void operator()()
		{
			start_read();
			return;
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
		int buffer_length;
		char buffer[1024];

		//listen on the socket connection for a message to come through
		void start_read()
		{
			buffer_length = 1024;
			while(1)
			{
				//bzero clears out the buffer
				bzero(buffer, buffer_length);
				//read will read the message off the socket connection and
				//store the message in buffer
				n = read(newsockfd, buffer, buffer_length);
				if(n==0)
				{
					close_con();
					return;
				}
				if(n<0) error("ERROR reading from socket");

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
					case LEAVE: close_con();
											return;
					case ERROR: "";
											break;
				}
				
				int rs_len = serv_resp.length();
				char rspns[rs_len]; 
				bzero(rspns, rs_len);
				size_t length = serv_resp.copy(rspns,rs_len, 0);
				rspns[length] = '\0';
				//here is where we will write back to the client the correct msg
				//we can either keep it here and pass back from the methods we call
				//what needs to be sent to the client or just send the message from
				//the method called and remove this call to start_write()
				n = write(newsockfd, rspns, rs_len);
				cout << "\nwrote to socket:\n" << rspns << "\n" << n << "\n" << endl;
				if(n==0)
				{
					close_con();
					return;
				}
			  if(n<0) error("Error writing to socket");
				//clear out the buffer (idk if we need it but it was causing some
				//issues when i wasn't
				bzero(buffer,rs_len);
			}
		}

		//gracefully close the connection when either the client sends a 
		//message to disconnect or the client side just decides to close
		void close_con()
		{
			printf("closed connection: %d\n", con_num);
			close(newsockfd);
		}
};
//==========================================================================main
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
	//loop here and conitnually listen for connections the 5 in the parameters 
	//is the amount of connection requests that can be in queue at one time if
	//there for some reason is a problem with the socket accepting the client
	//connection
	int con_num = 0;
	while(1)
	{
		listen(sockfd, 1);

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
	//close the server socket
	cout << "Exited while loop and closing" << endl;
	close(sockfd);
	return 0;
}
