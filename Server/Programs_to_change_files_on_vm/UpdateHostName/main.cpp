#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#define HOSTS_FILE "/etc/hosts"
#define HOSTNAME_FILE "/etc/hostname"

void println(const std::string& output);
bool validateHostname(char *hostname);


int main(int argc, char **argv) {
    //Validate that input has been given
    if(argc < 2){
        println("The program takes the new hostname as first argument");
        return 1;
    }
    if(!validateHostname(argv[1])) return 1;//validateHostname prints its own debug messages

    std::string oldHostName;
    std::string newHostName = argv[1];
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
            println("Error on write /etc/hostname");
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
            println("Error on write /etc/hosts");
            outputFileStream.close();
            return 1;
        }else{
            return 0;
        }
    }
}

void println(const std::string& output){
    std::cout << output << std::endl;
}

bool validateHostname(char *hostname){
    //Validate Hostname
    char *newHostName = hostname;
    int newHostNameSize = (sizeof(newHostName)/sizeof(newHostName[0]));
    if(newHostNameSize<1 || newHostNameSize > 63){
        println("The hostname cannot be smaller than 1 character or greater than 63 characters");
        return false;
    }
    if(newHostName[0] == '-'){
        println("Hostname Cannot start with a '-'");
        return false;
    }

    for(int i = 0 ; i < newHostNameSize; i++){
        if(!( //If char is not one of the following then:
                (newHostName[i] >= 97 && newHostName[i] <= 122) ||  //char is a-z
                newHostName[i] == '-' ||                            //char is -
                (newHostName[i] >= 48 && newHostName[i] <= 57 ))){    //char is 0-9
            println("Hostname can only contain the characters a-z and '-' and cannot start with '-'");
            return false;
        }
    }
    return true;
}