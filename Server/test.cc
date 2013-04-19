#include <cstdlib>
#include <string>
#include <iostream>
int main()
{
  std::string test = "CHANGE \nName:name \nVersion:version \nCell:cell \nLength:length \ncontent \n";

  std::cout << test << std::endl;

  test.erase(0, 6);

  std::cout << test << std::endl;
  test.insert(0, "UPDATE");
  std::cout << test << std::endl;

  return 0;
}
