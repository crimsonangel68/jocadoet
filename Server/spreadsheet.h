/* A spreadsheet object that will keep track of the state of a spreadsheet
 *
 *
 * Authors: Doug Hitchcock, Joshua Boren, Ethan Hayes, Calvin Kern
 *
 *
 */

#ifndef SPREADSHEET_H
#define SPREADSHEET_H

#include <string>
#include <map>
#include <set>
#include <cstdlib>
#include <list>


class spreadsheet
{
 public:
  // Constructor
  spreadsheet(std::string n, std::string pass, int initialVersion);

  // Copy Constructor
  spreadsheet(const spreadsheet & other);

  // Destructor
  ~spreadsheet();

  // Methods

  bool check_password(std::string pass);
  bool check_version(int testVersion);
  std::string get_XML();
  void edit_cell_content(std::string cellName, std::string cellContent);
  std::string get_name() const;
  std::string get_password() const;
  int get_version() const;
  std::map<std::string, std::string> get_cells() const;
  std::list<int> get_clients() const;
  void add_client(int c);
  void write_file();

  // Member Variables
 private:
  std::string name;
  std::string password;
  int SSVersion;
  std::map<std::string, std::string> cells;
  std::list<int> clients;

  
};
#endif
