/* This is an object class to represent the state of a spreadsheet
 *
 * Authors: Doug Hitchcock, Joshua Boren, Ethan Hayes, Calvin Kern
 *
 */
#include <iostream>
#include <sstream>
#include <cstdlib>
#include <string>
#include "spreadsheet.h"
#include <list>
#include <fstream>

//=============================  Constructors  ================================
// Base Constructor
spreadsheet::spreadsheet (std::string n, std::string pass, int initialVersion)
{
	this->name = n;
	this->password = pass;
	this->SSVersion = initialVersion;
	this->cells = initializeCellMap();
	this->clients = clients;
}

// Construct spreadsheet from filename
spreadsheet::spreadsheet(std::string file)
{

	this->cells = initializeCellMap();
	this->cells = openCellMap(file);

}

// Copy Constructor
spreadsheet::spreadsheet (const spreadsheet & other)
{
	this->name = other.get_name();
	this->password = other.get_password();
	this->SSVersion = other.get_version();
	this->cells = other.get_cells();
	this->clients = other.get_clients();
}

//Destructor
spreadsheet::~spreadsheet ()
{
}

// --------------------------- Helper  Methods ---------------------------------

// check password provided against spreadsheet password
bool spreadsheet::check_password(std::string pass) 
{ 
	return (this->password == pass);  
}

// check version provided against spreadsheet version
bool spreadsheet::check_version(int testVersion) 
{ 
	return (this->SSVersion == testVersion);  
}

// Get name of spreadsheet
std::string spreadsheet::get_name() const { return this->name; }

// Get password of spreadsheet
std::string spreadsheet::get_password() const { return this->password; }

// Get version of spreadsheet
int spreadsheet::get_version() const { return this->SSVersion; }

// Get cells of spreadsheet
std::map<std::string, std::string> spreadsheet::get_cells() const 
{
	return this->cells; 
}

// Get clients of spreadsheet
std::list<int> spreadsheet::get_clients() const{ return this->clients; }

// add client to list of clients on spreadsheet
void spreadsheet::add_client(int c){ this->clients.push_front(c); }

// Set the name of the spreadsheet
void spreadsheet::set_name(std::string n) { this->name = n; }

// Set the password of the spreadsheet
void spreadsheet::set_password(std::string pw) { this->password = pw; }

// Set the version of the spreadsheet
void spreadsheet::set_version(int version) { this->SSVersion = version; }

// get deque
std::deque<std::pair<std::string, std::string> > spreadsheet::get_undoQUE() const
{
	return this->undoQUE;
}

// --------------------- XML and write to file methods -------------------------

// Get XML to write to file
std::string spreadsheet::get_XML()
{
	std::string xml;
	std::stringstream ss;
	ss << "<spreadsheet name>\n" << this->name << "\n</spreadsheet name>\n";
	ss << "<password>\n" << this->password << "\n</password>\n";
	ss << "<version>\n" << this->SSVersion << "\n</version>\n";
	ss << "<?xml version=\"" << SSVersion << ".0\" encoding=\"utf-8\"?>\n";
	ss << "<spreadsheet version=\"ps6\">\n";
	std::map<std::string, std::string>::iterator it;
	for(it = this->cells.begin(); it != this->cells.end(); ++it)
	{
		ss << "<cell>\n<name>\n" << (*it).first << "\n</name>\n<contents>\n";
		ss << (*it).second << "\n</contents>\n</cell>\n";
	}
	ss << "</spreadsheet>\n";
	xml = ss.str();
	return xml;

}

// Get XML to send to users who are connecting
std::string spreadsheet::get_XML_for_user()
{
	std::string xml;
	std::stringstream ss;
	ss << "<?xml version=\"" << SSVersion << ".0\" encoding=\"utf-8\"?>";
	ss << "<spreadsheet version=\"ps6\">";
	std::map<std::string, std::string>::iterator it;
	for(it = this->cells.begin(); it != this->cells.end(); ++it)
	{
		ss << "<cell><name>" << (*it).first << "</name><contents>";
		ss << (*it).second << "</contents></cell>";
	}
	ss << "</spreadsheet>\n";
	xml = ss.str();
	return xml;
}

// write the XML to file
void spreadsheet::write_file(std::string SSname)
{
	std::string xml = get_XML();
	//std::cout << xml << std::endl;
	std::stringstream ss;
	ss << SSname << ".ss";
	std::string file = ss.str();
	char buffer[256];
	std::size_t length = file.copy(buffer,256, 0);
	buffer[length] ='\0';
	std::ofstream tempFile(buffer);
	tempFile << xml;
	tempFile.close();
}

//===============================  Edit / Undo  ================================
void spreadsheet::edit_cell_content(std::string cellName, std::string cellContent)
{
	std::map<std::string, std::string>::iterator it;
	std::string mapValue = cells.find(cellName)->second = cellContent;
	SSVersion++;
}

void spreadsheet::add_undo(std::string cellName, std::string cellContent)
{
	std::pair<std::string, std::string> undoPAIR = std::make_pair (cellName, cellContent);
	this->undoQUE.push_back(undoPAIR);
	if(this->undoQUE.size() > 10)
	{
		this->undoQUE.pop_front();
	}
}

//gets the last change made to the spreadsheet to perform the undo function
std::string spreadsheet::get_undo()
{
	std::pair<std::string, std::string> undoPAIR = this->undoQUE.back();
	this->undoQUE.pop_back();
	std::string cellName = undoPAIR.first;
	std::string cellContent = undoPAIR.second;

	std::stringstream convert;
	convert << cellContent.size();
	std::string result = convert.str();

	std::stringstream convert2;
	convert2 << this->get_version();
	std::string result2 = convert2.str();

	std::stringstream undoMessage;

	undoMessage << "UNDO SP OK \n";
	undoMessage << "Name:" + this->name + " \n";
	undoMessage << "Version:";
	undoMessage << convert2;
	undoMessage << " \n";
	undoMessage << "Cell:"+cellName+" \n";
	undoMessage << "Length:";
	undoMessage << convert;
	undoMessage << " \n";
	undoMessage << cellContent + " \n";

	return undoMessage.str();
}


// -------------------- Initialize and Opencell map methods --------------------
// Initialize all the cells in a new spreadsheet to empty strings
std::map<std::string, std::string> spreadsheet::initializeCellMap()
{
	std::string abc[] = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
	std::map<std::string, std::string> tempCells;
	for(int i = 0; i < 26; i++)
	{
		for(int j = 1; j < 100; j++)
		{
			std::stringstream ss;
			ss << abc[i];
			ss << j;
			std::string cell = ss.str();
			std::string content = "";
			if(tempCells.find(cell) == tempCells.end())
				tempCells.insert(std::pair< std::string, std::string> (cell, content));
		}
	}
	return tempCells;
}

// Open a spreadsheet and generate the cell map from the XML
std::map<std::string, std::string> spreadsheet::openCellMap(std::string file)
{
	std::map<std::string, std::string> tempCells;
	char buffer[256];
	std::size_t length = file.copy(buffer,256, 0);
	buffer[length] ='\0';
	std::ifstream in(buffer);
	std::string line;

	while(true)
	{
		getline(in, line);
		std::stringstream ss(line);
		if(ss.str() == "<spreadsheet name>")
		{
			getline(in, line);
			this->set_name(line);
		}
		else if(ss.str() == "<password>")
		{
			getline(in, line);
			this->set_password(line);
		}
		else if(ss.str() == "<version>")
		{
			getline(in, line);
			this->set_version(atoi(line.c_str()));
		}
		if(ss.str() == "<cell>")
		{
			getline(in, line);
			getline(in, line);

			std::string cellName = line;
			getline(in, line);
			getline(in, line);
			getline(in, line);
			std::string cellContent = line;
			if(tempCells.find(cellName) == tempCells.end())
			{
				tempCells.insert(std::pair< std::string, std::string> (cellName, cellContent));
			}
			std::cout << cellName << " : " << cellContent << std::endl;
		}
		if(ss.str() == "</spreadsheet>")
			break;
	}
	in.close();

	return tempCells;
}
