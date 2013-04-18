#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include <sstream>
#include <cstdlib>
#include <algorithm>
#include <iterator>
#include <set>
#include <map>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <boost/thread.hpp>
#include <boost/date_time.hpp>

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

//prints out an error message
void error(const char *msg)
{
	perror(msg);
	exit(1);
}

//unexpected client disconnect
void bad_disc(int newsockfd)
{
	close(newsockfd);
}


//=======================parser

vector<string> changeCommand(string change)
{
  vector<string> info;
  stringstream ss(change);
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
      	string serverResponse = serverResponseSS.str();
	{
	  // Loop through all clients
      	std::cout << serverResponse << std::endl;
	}
      }
      else if(testVersionEqualsSpreadsheetVersion)
	{
	  stringstream serverResponseSS;
	  int SSversion = testVersion;
	  serverResponseSS << "CHANGE WAIT OK \n";
	  serverResponseSS << info[1];
	  serverResponseSS << " \n";
	  serverResponseSS << "Version:";
	  serverResponseSS << SSversion;
	  serverResponseSS << " \n";
	  string serverResponse = serverResponseSS.str();
	  std::cout << serverResponse << std::endl;
	}
	if(true)
       	{
	  stringstream serverResponseSS;
	  int SSversion = testVersion;
	  serverResponseSS << "CHANGE WAIT OK \n";
	  serverResponseSS << info[1];
	  serverResponseSS << "\n";
	  serverResponseSS << "MESSAGE REGARDING FAIL\n";
	  string serverResponse = serverResponseSS.str();
	  std::cout << serverResponse << std::endl;
	}


    return info;
    
}

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

vector<string> createCommand(string create)
{

  vector<string> info;
  stringstream ss(create);
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
    bool testNameNotTaken = false; // Test if file name exists already
    

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
	
      	string serverResponse = serverResponseSS.str();

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
	
      	string serverResponse = serverResponseSS.str();

      	std::cout << serverResponse << std::endl;
      }
    
    return info;
}

vector<string> joinCommand(string join)
{
  
  vector<string> info;
  stringstream ss(join);
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
    bool passwordMatches = false; // Check if password matches
    
    if(nameExists && passwordMatches)
      {

	// Retrieve spreadsheet information ------------------- Need to implement -------------
	int SSversion = 1029; // Get current version number of spreadsheet
	int lengthOfSpreadsheetXML = 1313; // lengthOfSpreadsheetXML = SpreadsheetXML.length();
	std::string xml = ""; // xml = readtextfile(tempName);
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
	
	
      	string serverResponse = serverResponseSS.str();

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
	
      	string serverResponse = serverResponseSS.str();

      	std::cout << serverResponse << std::endl;
      }



  return info;
}


//=======================parser=======================

		//Creates a connection object that runs on it's own thread
		//this class will have all the functionality that each 
		//connection that is made will need
	class Connection 
{
	public:
		//constructor
		//takes in the socket file descriptor for the connection
		Connection(int c_newsockfd) 
			: newsockfd(c_newsockfd)
		{
			//in here we can put the information for the connection 
			//in a map/list to keep track of live connections
		}

		//this method is necessary for multithreading. Starts listening
		//on the socket for messages to come through
		void operator()()
		{
			start_read();
		}

	private:
		int n, newsockfd;
		char buffer[256];
		string name;

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
				bad_disc(newsockfd);
				return;
			}
			if(n<0) error("ERROR reading from socket");
			//printf("%s",buffer);

			//here is where we need to parse the message to determine what
			//command has been sent by the client

			

			//here is where we will write back to the client the correct msg
			//we can either keep it here and pass back from the methods we call
			//what needs to be sent to the client or just send the message from
			//the method called and remove this call to start_write()
			start_write(cmd);
			//clear out the buffer (idk if we need it but it was causing some
			//issues when i wasn't
			bzero(buffer,256);
			//start listening on the socket connection again
			start_read();
		}

		//write the message to the client on the socket connection
		void start_write(int cmd)
		{

			n = write(newsockfd, "Got it", 18);
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
		Connection c(newsockfd);
		//send it to it's own thread
		boost::thread conThread(c);

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
