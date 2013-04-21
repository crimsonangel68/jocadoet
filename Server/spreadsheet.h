/* A spreadsheet object that will keep track of the state of a spreadsheet
 *
 * Authors: Doug Hitchcock, Joshua Boren, Ethan Hayes, Calvin Kern
 *
 */
#ifndef SPREADSHEET_H
#define SPREADSHEET_H

#include <string>
#include <map>
#include <set>
#include <cstdlib>
#include <list>
#include <deque>

//Class to represent a spreadsheet
class spreadsheet
{
	public:
		// Constructor (for new spreadsheets)
		spreadsheet(std::string n, std::string pass, int initialVersion);
		spreadsheet(std::string file);

		// Copy Constructor
		spreadsheet(const spreadsheet & other);

		// Destructor
		~spreadsheet();

		// Methods
		
		std::map<std::string, std::string> initializeCellMap();
		std::map<std::string, std::string> openCellMap(std::string file);
		std::map<std::string, std::string> get_cells() const;
		std::list<int> get_clients() const;
		std::deque<std::pair<std::string, std::string> > get_undoQUE() const;
		std::string get_XML();
		std::string get_XML_for_user();
		std::string get_name() const;
		std::string get_password() const;
		std::string get_undo();
		int get_version() const;
		bool check_password(std::string pass);
		bool check_version(int testVersion);
		bool check_queue();
		void edit_cell_content(std::string cellName, std::string cellContent);
		void set_name(std::string n);
		void set_password(std::string pw);
		void set_version(int version);
		void add_client(int c);
		void write_file(std::string file);
		void add_undo(std::string cellName, std::string cellContent);
		void clear_undo();
		void remove_client(int client);

		// Member Variables
	private:
		std::deque<std::pair<std::string, std::string> > undoQUE;
		std::map<std::string, std::string> cells;
		std::list<int> clients;
		std::string name;
		std::string password;
		int SSVersion;
};
#endif
