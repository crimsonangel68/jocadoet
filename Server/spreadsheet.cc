/* This is an object class to represent the state of a spreadsheet
 *
 *
 *
 * Authors: Doug Hitchcock, Joshua Boren, Ethan Hayes, Calvin Kern
 *
 */

#include <iostream>
#include <sstream>
#include "spreadsheet.h"
#include <fstream>


std::map<std::string, std::string> initializeCellMap();

// Base Constructor
spreadsheet::spreadsheet (std::string n, std::string pass, int initialVersion)
{
  this->name = n;
  this->password = pass;
  this->SSVersion = initialVersion;
  this->cells = initializeCellMap();
  this->clients = clients;
}

// Copy Constructor
spreadsheet::spreadsheet (const spreadsheet & other)
{
  this->name = other.get_name();
  this->password = other.get_password();
  this->SSVersion = other.get_version();
  this->cells = other.get_cells();
  this->clients = other.get_clients();
  this->undoQUE = other.get_undoQUE();
}

//Destructor
spreadsheet::~spreadsheet ()
{
}

// Methods
  bool spreadsheet::check_password(std::string pass)
  {
    return (this->password == pass);
  }

  bool spreadsheet::check_version(int testVersion)
  {
    return (this->SSVersion == testVersion);
  }

std::string spreadsheet::get_XML()
  {
    std::string xml;
    std::stringstream ss;
    // ss << this->name;
    // ss << this->password;
    // ss << this->SSVersion;
    ss << "<?xml version=\"" << SSVersion << ".0\" encoding=\"utf-8\"?>";
    ss << "<spreadsheet version=\"ps6\">";
    std::map<std::string, std::string>::iterator it;
    for(it = this->cells.begin(); it != this->cells.end(); ++it)
    {
      ss << "<cell><name>" << (*it).first << "</name><contents>" << (*it).second << "</contents></cell>";
    }
    ss << "</spreadsheet>\n";
    xml = ss.str();
    return xml;
    
  }

void spreadsheet::write_file()
{
  std::string xml = get_XML();
  std::ofstream tempFile("helloCalvin.ss");
  tempFile << xml;
  tempFile.close();
}

  void spreadsheet::edit_cell_content(std::string cellName, std::string cellContent)
  {
    std::map<std::string, std::string>::iterator it;
    std::string mapValue = cells.find(cellName)->second = cellContent;
    
  }


  std::string spreadsheet::get_name() const
  {
    return this->name;
  }


  std::string spreadsheet::get_password() const
  {
    return this->password;
  }


  int spreadsheet::get_version() const
  {
    return this->SSVersion;
  }


std::map<std::string, std::string> spreadsheet::get_cells() const
  {
    return this->cells;
  }

std::list<int> spreadsheet::get_clients() const
{
  return this->clients;
}

std::deque<std::pair<std::string, std::string> > spreadsheet::get_undoQUE() const
{
  return this->undoQUE;
}


void spreadsheet::add_client(int c)
{
  this->clients.push_front(c);
}

void spreadsheet::add_undo(std::string cellName, std::string cellContent)
{
  std::pair<std::string, std::string> undoPAIR = std::make_pair (cellName, cellContent);
  this->undoQUE.push_back(undoPAIR);
  if (this->undoQUE.size() > 10)
    {
      this->undoQUE.pop_front();
    }
}

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
  undoMessage <<"Cell:" + cellName + " \n";
  undoMessage << "Length:";
  undoMessage << convert;
  undoMessage <<" \n";
  undoMessage <<  cellContent + " \n";

  return undoMessage.str();
}

std::map<std::string, std::string> initializeCellMap()
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
	    {
	      tempCells.insert(std::pair< std::string, std::string> (cell, content));
	    }
	  
	}
    }
  return tempCells;
}

// int main()
// {
//   std::cout << "Is it working yet?" << std::endl;
//   std::cout << "Is the map working?" << std::endl;
//   std::map<std::string, std::string> tempCells = initializeCellMap();
//   std::map<std::string, std::string>::iterator it;
//   spreadsheet tempSS("Calvin", "poop", 1);
//   for(it = tempCells.begin(); it != tempCells.end(); ++it)
//     {
//       std::cout << (*it).first << " " << (*it).second <<std::endl;
//     }
//   tempSS.write_file();
//   return 0;
// }



