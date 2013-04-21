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
	cout << "need to catch/deal with exception" << endl;
	exit(1);
}

//================================================================updateCommand
//Takes an update command that is received on a given connection and
//broadcasts the updates to all of the clients connected to the indicated
//spreadsheet name
void updateCommand(string update, int connection, string SSname)
{
	int n;
	update.erase(0, 6);
	update.insert(0, "UPDATE");
	// LOOP THROUGH CONNECTIONS AND SEND IT TO ALL OTHER connections except the 
	// one that sent the change
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
			//write the update to the connection because it is tied
			//to this spreadsheet
			n = write(tempConnection, buffer, update.length()+1);
			//error check for write to socket write returns 0 if there is no
			//connection on the given socket file descriptor
			if(n==0)
			{
				close(tempConnection);
				return;
			}
			if(n < 0)
			{
				cout << "updateCommand[89]" << endl;
				error("Error writing to socket");
			}
		}
		temp.pop_front();
		temp.push_back(tempConnection);
	}
}

//================================================================changeCommand
//Takes a change command that is received on a given connection and 
//determines if the change is a valid change to the spreadsheet.  If
//the request is valid or not the corresponding message will be returned.
string changeCommand(string change, int connection)
{
	//cout << change << "This is what I received" << endl;
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
	
	stringstream versionSS(info[2]);
	string version;

	while(getline(versionSS, version, ':' ))
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

	//cout << tempName << "\n" <<  version << "\n" <<  cellName << "\n" <<  cellContent << endl;

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
	//client does not have the correct version
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
	}
	//i don't see how this code will ever be reached....
	else
	{
		stringstream serverResponseSS;
		int SSversion = testVersion;
		serverResponseSS << "CHANGE SP FAIL \n";
		serverResponseSS << info[1];
		serverResponseSS << "\n";
		serverResponseSS << "MESSAGE REGARDING FAIL\n";
		serverResponse = serverResponseSS.str();
	}
	return serverResponse;
}

//================================================================undoCommand
//Takes an undo command that is received on a given connection and 
//determines if the undo request is valid or not and returns the 
//corresponding response to the client
vector<string> undoCommand(string undo)
{
  vector<string> info;
  stringstream ss(undo);
  string item;
  while(getline(ss, item))
    {
      info.push_back(item);
    }

    std::cout << info[1] << "\n" << info[2] << std::endl;
    stringstream tempSS(info[2]);
    vector<string> versionInfo;
    string temp1;
    while(getline(tempSS, temp1, ':' ))
      {
	versionInfo.push_back(temp1);
      }
    unsigned pos = temp1.find(" ");
    temp1 = temp1.substr(0, pos);
    int testVersion = atoi(temp1.c_str());
    std::cout << "temp1 is: " << temp1 << std::endl << std::endl << testVersion << std::endl;

    //if(testVersion == "spreadsheet version")  // --------------------- Need to integrate with server -----------
     {
      	      	// increment spreadsheet version
      	int SSversion = testVersion +1;
      	stringstream serverResponseSS;
      	serverResponseSS << "UNDO SP OK \n";
      	serverResponseSS << info[1];
      	serverResponseSS << " \n";
      	serverResponseSS << "Version:";
      	serverResponseSS << SSversion;  // --------------------- Need to integrate with server -----------
      	serverResponseSS << " \n";
      	serverResponseSS << "Cell:";
      	serverResponseSS << "CELL TO BE REPLACED";  // --------------------- Need to integrate with server -----------
      	serverResponseSS << "\n";
      	serverResponseSS << "Length:";
      	serverResponseSS << "Length of content"; // --------------------- Need to integrate with server -----------
      	serverResponseSS << "\n";
      	serverResponseSS << "OLD CONTENT OF CELL";  // --------------------- Need to integrate with server -----------
      	serverResponseSS << "\n";
	
      	string serverResponse = serverResponseSS.str();
	 {
	   // Loop through all clients connected to spreadsheet and send response
	   std::cout << serverResponse << std::endl;
	 }
   }
    //else if (undoStack.size() == 0) // no undo to perform
    {
	      
      	int SSversion = testVersion;
      	stringstream serverResponseSS;
      	serverResponseSS << "UNDO SP END \n";
      	serverResponseSS << info[1];
      	serverResponseSS << " \n";
      	serverResponseSS << "Version:";
      	serverResponseSS << SSversion;  // --------------------- Need to integrate with server -----------
      	serverResponseSS << " \n";
	
      	string serverResponse = serverResponseSS.str();
	{
	  // Send message to initial client
      	std::cout << serverResponse << std::endl;
	}
    }
    // else if(testVersion != SSversion)
     {
	      
      	int SSversion = testVersion;
      	stringstream serverResponseSS;
      	serverResponseSS << "UNDO SP WAIT \n";
      	serverResponseSS << info[1];
      	serverResponseSS << " \n";
      	serverResponseSS << "Version:";
      	serverResponseSS << SSversion;  // --------------------- Need to integrate with server -----------
      	serverResponseSS << " \n";
	
      	string serverResponse = serverResponseSS.str();
	{
	  // Send message to initial client
      	std::cout << serverResponse << std::endl;
	}
    }
     //else //Some other error
     {
	      
      	int SSversion = testVersion;
      	stringstream serverResponseSS;
      	serverResponseSS << "UNDO SP FAIL \n";
      	serverResponseSS << info[1];
      	serverResponseSS << " \n";
      	serverResponseSS << "UNDO failed for reason:"; // Message for reason for fail
      	serverResponseSS << " \n";
	
      	string serverResponse = serverResponseSS.str();

      	std::cout << serverResponse << std::endl;
     }

    
    return info;
}

//================================================================createCommand
//Takes a create command that is received on a given connection and 
//determines if the request is valid or not and returns the corresponding
//response to the client.
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

	//std::cout << info[1] << "\n" << info[2] << std::endl;
	stringstream nameSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	while(getline(nameSS, tempName, ':' ))
	{
		nameInfo.push_back(tempName);
	}
	unsigned pos = tempName.find(" ");
	tempName = tempName.substr(0, pos);

	// Add name and Connection to the map
	list<int> connList;
	ss_connections.insert ( std::pair<std::string, list<int> >(tempName, connList));
	//cout<< "inserting into map" << endl;
	ss_connections.at(tempName).push_front(connection);

	stringstream passwordSS(info[2]);
	vector<string> passwordInfo;
	string tempPassword;
	while(getline(passwordSS, tempPassword, ':' ))
	{
		nameInfo.push_back(tempPassword);
	}
	pos = tempPassword.find(" ");
	tempPassword = tempPassword.substr(0, pos);
	//std::cout << "tempName is: " << tempName << std::endl << "tempPassword is: " << tempPassword << std::endl << std::endl;
	bool testNameNotTaken = true; // Test if file name exists already

	// Add spreadsheet to list of existing spreadsheets
	spreadsheet::spreadsheet newSS(tempName, tempPassword, 1);
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

		//std::cout << serverResponse << std::endl;
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

	//	std::cout << serverResponse << std::endl;
	}

	return serverResponse;
}

//================================================================joinCommand
//Takes a join command that is received from the given client and 
//determines if the join request is valid or not and responds with
//the corresponding message.
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

	//std::cout << info[1] << "\n" << info[2] << std::endl;

	stringstream nameSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	while(getline(nameSS, tempName, ':' ))
	{
		nameInfo.push_back(tempName);
	}
	unsigned pos = tempName.find(" ");
	tempName = tempName.substr(0, pos);

	// Add name and Connection to the map
	list<int> connList;
	ss_connections.insert ( std::pair<std::string, list<int> >(tempName, connList));
	//cout<< "inserting int map join" << endl;
	ss_connections.at(tempName).push_front(connection);

	stringstream passwordSS(info[2]);
	vector<string> passwordInfo;
	string tempPassword;
	while(getline(passwordSS, tempPassword, ':' ))
	{
		nameInfo.push_back(tempPassword);
	}
	pos = tempPassword.find(" ");
	tempPassword = tempPassword.substr(0, pos);

	bool nameExists = false; // Check to see if name exists
	bool passwordMatches = false; // Check if password matches

	spreadsheet sheet(tempName, tempPassword, 1);

	for(int i = 0; i < connected_ss.size(); i++)
	{
		if(tempName == connected_ss[i].get_name())
		{
			sheet = connected_ss[i];
			nameExists = true;
		}
	}

	passwordMatches = sheet.check_password(tempPassword);

	if(nameExists && passwordMatches)
	{
		// Retrieve spreadsheet information
		int SSversion = sheet.get_version(); // Get current version number of spreadsheet
		std::string xml = sheet.get_XML_for_user();
		int lengthOfSpreadsheetXML = xml.length();
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

		//std::cout << serverResponse << std::endl;
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

		//std::cout << serverResponse << std::endl;
	}
	return serverResponse;
}

//================================================================saveCommand
//Takes a save message from a given client and determines if the
//request is a valid one and returns the corresponding response
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

	//std::cout << info[1] << "\n" << info[2] << std::endl;
	stringstream nameSS(info[1]);
	vector<string> nameInfo;
	string tempName;
	//cout << "herehere" << endl;
	while(getline(nameSS, tempName, ':' ))
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

		//std::cout << serverResponse << std::endl;
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

		//std::cout << serverResponse << std::endl;
	}

	return serverResponse;
}

//================================================================parse
//Determines by the first characters of the message what type of
//message has been received in order to call the correct method
//to handle the message
int parse(char buf[256])
{
	if(buf[0] == 'C')
		if(buf[1] == 'R')
			return CREATE;
		else if(buf[1] == 'H')
			return CHANGE;

	if(buf[0] == 'J')
		return JOIN;

	if(buf[0] == 'U')
		if(buf[1] == 'N')
			return UNDO;
		else if(buf[1] == 'P')
			return UPDATE;

	if(buf[0] == 'S')
		return SAVE;

	if(buf[0] == 'L')
		return LEAVE;

	return ERROR;
}


//================================================================Connection
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
				if(n<=0)
				{
					close_con();
					return;
				}
				//advertise to server administrator what was received on the given
				//connection number
				cout << "\n==============\n--------------\nreceived from connection# ";
				cout <<con_num<<"\n"<<buffer<<endl;	

				//determine what command was received on this connection
				int cmd = parse(buffer);
				
				string message = string(buffer);
				string serv_resp = "ERROR \n";
				//depending on the message that is received on this connection
				//call the corresponding method to handle the request.
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
				
				//prepare the response to write back to the client
				int rs_len = serv_resp.length();
				char rspns[rs_len]; 
				bzero(rspns, rs_len);
				size_t length = serv_resp.copy(rspns,rs_len, 0);
				//add the terminating char to the end of the buffer
				rspns[length] = '\0';
				//Write the response to the client and have the error check
				//if the write call returns 0, the client is no longer
				//connected if it is less than 0 a different error occured
				n = write(newsockfd, rspns, rs_len+1);
				if(n<=0)
				{
					close_con();
					return;
				}
				//advertise to the server administrator what was wrote back to 
				//the client on this connection
				cout << "\nwrote to socket:\n" << rspns << "\nLength: ";
				cout << n << "\n------------------\n==================\n" << endl;
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
