//
// Created by frederik on 3/21/21.
//

//Project headers
#include "hostname.h"
#include "print.h"
//Third party headers

//STD headers
#include <string>
#include <iostream>
#include <fstream>
#include <vector>
#include <regex>


namespace scalable{

const char* HOSTS_FILE = "/etc/hosts";
const char* HOSTNAME_FILE = "/etc/hostname";

    bool validateHostname(const std::string &hostname){
        int newHostNameSize = hostname.length();
        if(newHostNameSize<1 || newHostNameSize > 63){
            scalable::println("The hostname cannot be smaller than 1 character or greater than 63 characters");
            return false;
        }
        if(hostname.at(0) == '-'){
            scalable::println("Hostname Cannot start with a '-'");
            return false;
        }

        //Match any string starting with a-z, followed by 0 to 62 instances of 0-9 or a-z or - , any more and it will be ignored
        std::regex regex ("(^[a-z][a-z0-9-]{0,62}$)", std::regex_constants::ECMAScript);
        return std::regex_search(hostname, regex);
    }

/// Errors:
/// 1: Error on write /etc/hostname
/// 2: Error on write /etc/hosts
/// \param newHostName
/// \return success state 0 if successful
    int updateHostname(const std::string &newHostName){
        std::string oldHostName;
        {
            //Read Current Contents
            std::ifstream inputFileStream(HOSTNAME_FILE, std::ios::in);
            std::getline(inputFileStream, oldHostName);
            inputFileStream.close();

            //Write new name
            std::fstream outputFileStream(HOSTNAME_FILE, std::ios::out | std::ios::trunc);
            outputFileStream.write(newHostName.c_str(), newHostName.length());
            outputFileStream.flush();
            if(outputFileStream.bad()){
                perror("Error on write /etc/hostname");
                outputFileStream.close();
                return 1;
            }
            outputFileStream.close();
        }

        {
            std::vector<std::string> lineVector = std::vector<std::string>(); //Create a new empty vector
            std::string line; // Pre declare the line for scanning lines
            std::ifstream inputFileStream(HOSTS_FILE, std::ios::in);
            while (std::getline(inputFileStream, line)){
                int indexOfHostName = static_cast<int>(line.find(oldHostName));
                if(indexOfHostName < line.length()){ //Found hostname in line
                    std::string newLine = std::string("");
                    newLine.append(line.substr(0, indexOfHostName));
                    newLine.append(newHostName);
                    newLine.append("\n");
                    lineVector.push_back(newLine);
                }else{
                    line.append("\n");
                    lineVector.push_back(line);
                }
            }
            inputFileStream.close();
            std::ofstream outputFileStream(HOSTS_FILE, std::ios::out | std::ios::trunc);
            for(const auto& l : lineVector){
                outputFileStream.write(l.c_str(),l.length());
            }
            outputFileStream.flush();
            if(outputFileStream.bad()){
                perror("Error on write /etc/hosts");
                outputFileStream.close();
                return 2;
            }else{
                return 0;
            }
        }
    }
}