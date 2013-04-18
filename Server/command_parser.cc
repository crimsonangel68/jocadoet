#include <cstdlib>
#include <iostream>
#include <string>
#include <sstream>
#include <algorithm>
#include <iterator>
#include <set>
#include <map>
#include <vector>


using namespace std;

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

    

int main(int argc, char * args[])
{

  vector<string> change = changeCommand("CHANGE \nName:change \nVersion:1234 .\nCell:cell \nLength:length \ncontent \n");
  vector<string> undo = undoCommand("UNDO \nName:undo \nVersion:5678 \n");
  vector<string> create = createCommand("CREATE \nName:create \nPassword:password \n");
  vector<string> join = joinCommand("JOIN \nName:join \nPassword:password \n");
  
  return 0;

}

