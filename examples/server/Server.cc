/* A simple server in the internet domain using TCP
	 The port number is passed as an argument */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>

void error(const char *msg)
{
	perror(msg);
	exit(1);
}

int main(int argc, char *argv[])
{
	int sockfd, newsockfd, portno;
	socklen_t clilen;
	char buffer[256];
	struct sockaddr_in serv_addr, cli_addr;
	int n;

	//check arguments
	if (argc < 2) {
		fprintf(stderr,"ERROR, no port provided\n");
		exit(1);
	}

	//create a new socket
	sockfd = socket(AF_INET, SOCK_STREAM, 0);

	//make sure the socket was created properly
	if (sockfd < 0) 
		error("ERROR opening socket");

	//sets all values in a buffer to zero		
	bzero((char *) &serv_addr, sizeof(serv_addr));

	//save the port number the server side socket is to be saved to
	portno = atoi(argv[1]);

	/* The variable serv_addr is a structure of type struct sockaddr_in.  
	 * This structure has four fields.  
	 * The first field is short sin_family, 
	 * which contains a code for the address family.  
	 * It should always be set to the symbolic constant AF_INET	*/
	serv_addr.sin_family = AF_INET;

	/* the third field of sockaddr_in is a 
	 * structure of type struct in_addr which contains 
	 * ony a single field unsigned long s_addr.  
	 * This field contains the IP address of the host.  
	 * For server code, this will always be the 
	 * IP address of the machine on which the server is running, 
	 * and there is a symbolic constant INADDR_ANY which gets this address.	*/
	serv_addr.sin_addr.s_addr = INADDR_ANY;

	/* The second field of serv_addr is unsigned short sin_port, 
	 * which contain the port number.  However instead of simply 
	 * copying the port number to this field, it is necessary 
	 * to convert this to network byte order using the function htons() 
	 * which converts a port number in host byte order 
	 * to a port number in network byte order.	*/
	serv_addr.sin_port = htons(portno);

	/* the bind() system call binds a socket to an address, 
	 * in this case the address of the current host and port number 
	 * on which the server will run.  
	 * It takes three arguments, 
	 * the socket file descriptor, 
	 * the address to which it is bound, 
	 * and the size of the address to which it is bound. 
	 * The second argument is a pointer to a structure of type sockaddr, 
	 * but what is passed in is a structure of type sockaddr_in, 
	 * and so this must be cast to the correct type. 
	 * This can fail for a number of reasons, the most obvious being 
	 * that this socket is already in use on this machine. */
	if (bind(sockfd, (struct sockaddr *) &serv_addr,
				sizeof(serv_addr)) < 0) 
		error("ERROR on binding");

	

		/* The listen system call allows the process 
		 * to listen on the socket for connections.  
		 * The first argument is the socket file descriptor, 
		 * and the second is the size of the backlog queue, i.e., 
		 * the number of connections that can be waiting while 
		 * the process is handling a particular connection.  
		 * This should be set to 5, the maximum size permitted by most systems.  
		 * If the first argument is a valid socket, this call cannot fail, 
		 * and so the code doesn't check for errors.	*/
		listen(sockfd,3);

		/* the accept() system call causes the process to block 
		 * until a client connects to the server.  
		 * Thus, it wakes up the process when a 
		 * connection from a client has been successfully established.  
		 * It returns a new file descriptor, and 
		 * all communication on this connection should be 
		 * done using the new file descriptor.  
		 * The second argument is a reference pointer 
		 * to the address of the client on the other end of the connection, 
		 * and the third argument is the size of this structure.	*/
		clilen = sizeof(cli_addr);

		newsockfd = accept(sockfd, 
				(struct sockaddr *) &cli_addr, 
				&clilen);
		if (newsockfd < 0) 
			error("ERROR on accept");

		while(1)
		{

		/* Note that we would only get to this point 
		 * after a client has successfully connected to our server.  
		 * This code initializes the buffer using the bzero() function, 
		 * and then reads from the socket.  Note that the read call uses 
		 * the new file descriptor, the one returned by accept(), 
		 * not the original file descriptor return by socket().  
		 * Note also that the read() will block until there is something 
		 * for it to read in the socket, i.e. after the client has executed a write().	*/
		bzero(buffer,256);
		n = read(newsockfd,buffer,255);
		if (n < 0) error("ERROR reading from socket");
		printf("Here is the message: %s\n",buffer);

		/* Once a connection has been established, 
		 * both ends can both read and write to the connection.  
		 * Naturally, everything written by the client will be read by the server, 
		 * and everything written by the server will be read by the client.  
		 * This code simply writes a short message to the client.  
		 * The last argument of write is the size of the message.*/
		n = write(newsockfd,"I got your message",18);
		if (n < 0) error("ERROR writing to socket");

	}

	close(newsockfd);

	close(sockfd);

	return 0; 
}
