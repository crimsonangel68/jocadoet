#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <vector>
#include <sstream>
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

		//listen on the socket connection for a message to come through
		void start_read()
		{
			//bzero clears out the buffer
			bzero(buffer, 256);
			//read will read the message off the socket connection and
			//store the message in buffer
			n = read(newsockfd, buffer, 255);
			if(n<0) error("ERROR reading from socket");
			//printf("%s",buffer);
			
			//here is where we need to parse the message to determine what
			//command has been sent by the client
			//
			//i've been thinking of a design where we can call a method to 
			//parse or split the message into useful parts.  not only will
			//we want to split by newline but also by colons. For exame in 
			//CREATE LF
			//Name:josh LF
			//Password:james LF
			//we don't care about "Name:" and "Password:"
			//then when we know that call whatever methods we write to handle
			//the respective commands

			int cmd = parse(buffer);

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

		//parse/split the message into useful segments
		int parse(char buf[256])
		{
			
			printf("This was the msg: %s\n", buf);
			return ERROR;
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
	close(newsockfd);	
	//close the server socket
	close(sockfd);

	return 0;
}
