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

void error(const char *msg)
{
	perror(msg);
	exit(1);
}

class Connection 
{
	public:
		Connection(int c_newsockfd) 
			: newsockfd(c_newsockfd)
		{
		}

		void operator()()
		{
			start_read();
		}

	private:
		int n, newsockfd;
		char buffer[256];

		void start_read()
		{
			bzero(buffer, 256);
			n = read(newsockfd, buffer, 255);
			if(n<0) error("ERROR reading from socket");
			//printf("%s",buffer);
			const string *str = str(buffer);
			const string *del = "\n:";
			vector<string> *cmd = split(str);

			start_write(cmd);
			bzero(buffer,256);
			start_read();
		}

		void start_write(int cmd)
		{

			n = write(newsockfd, "Got it", 18);
			if(n<0) error("Error writing to socket");
		}

		int split(char buf[256])
		{
			
			printf("This was the msg: %s\n", buf);
			return ERROR;
		}

		void close_con()
		{
			close(newsockfd);
		}


};

int main(int argc, char* argv[])
{
	std::cout << "main: startup" << std::endl;
	int sockfd, newsockfd, portno;
	socklen_t clilen;
	struct sockaddr_in serv_addr, cli_addr;
	sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (sockfd < 0)
		error("ERROR opening socket");
	bzero((char *) &serv_addr, sizeof(serv_addr));
	portno = 1984;
	serv_addr.sin_family = AF_INET;
	serv_addr.sin_addr.s_addr = INADDR_ANY;
	serv_addr.sin_port = htons(portno);
	if (bind(sockfd, (struct sockaddr *) &serv_addr,
				sizeof(serv_addr)) < 0)
		error("ERROR on binding");
	//loop here
	while(1)
	{
		listen(sockfd, 5);

		clilen = sizeof(cli_addr);
		newsockfd = accept(sockfd,
				(struct sockaddr *) &cli_addr,
				&clilen);
		if(newsockfd < 0)
			error("ERROR on accept");

		Connection c(newsockfd);
		boost::thread conThread(c);

	}

	close(newsockfd);	
	close(sockfd);

	return 0;
}
