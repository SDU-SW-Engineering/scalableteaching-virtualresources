#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <boost/json.hpp>
#include <ctime>

#define HOSTS_FILE "/etc/hosts"
#define HOSTNAME_FILE "/etc/hostname"

namespace json = boost::json;

void println(const std::string& output);
bool validateHostname(const char *hostname);
int updateHostname(const char *newHostNameReference);
std::string randomAlphaNumString(const int& charCount);
std::string runAndGetResult(std::string command);
int addUser(std::string username, std::string userPassword, std::string &userPublicKey);
json::value parseConfigurationToJson(const char* configFile);
int parseConfiguration(json::value& config);


int main(int argc, char **argv) {

    //TODO: Validate hostname
    //updateHostname(argc, argv);
    //TODO: Add users
    //TODO: Install Packages
}
///
/// Errors:
/// 1: Invalid hostname
/// 2:
/// \param config
/// \return indicator of success 0 for no error
int parseConfiguration(json::value& config){
    //Validate Hostname
    const char* hostname = config.at("hostname").as_string().c_str();
    if(!validateHostname(hostname)) return 1;
    //Update Hostname
    updateHostname(hostname);

    //Create system user
    json::object systemUser = config.at("systemUser").as_object();
    addUser(systemUser.at())//TODO: Finish parsing config


}

json::value parseConfigurationToJson(const char* configFile){
    std::ifstream inputFileStream(configFile);
    if(inputFileStream.bad()){
        perror("Error occurred upon reading the configfile");
    }
    std::string fileText {std::istreambuf_iterator<char>(inputFileStream), std::istreambuf_iterator<char>()};
    inputFileStream.close();
    json::error_code jsonErrorCode;
    return json::parse(fileText, jsonErrorCode);
}

void println(const std::string& output){
    std::cout << output << std::endl;
}

bool validateHostname(const char *hostname){
    int newHostNameSize = (sizeof(hostname) / sizeof(hostname[0]));
    if(newHostNameSize<1 || newHostNameSize > 63){
        println("The hostname cannot be smaller than 1 character or greater than 63 characters");
        return false;
    }
    if(hostname[0] == '-'){
        println("Hostname Cannot start with a '-'");
        return false;
    }

    for(int i = 0 ; i < newHostNameSize; i++){
        if(!( //If char is not one of the following then:
                (hostname[i] >= 97 && hostname[i] <= 122) ||  //char is a-z
                hostname[i] == '-' ||                            //char is -
                (hostname[i] >= 48 && hostname[i] <= 57 ))){    //char is 0-9
            println("Hostname can only contain the characters a-z and '-' and cannot start with '-'");
            return false;
        }
    }
    return true;
}

/// Errors:
/// 1: Error on write /etc/hostname
/// 2: Error on write /etc/hosts
/// \param newHostName
/// \return success state 0 if successful
int updateHostname(const char *newHostNameReference){
    std::string newHostName(newHostNameReference);
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

int addUser(std::string& username, std::string& userPassword, std::string &userPublicKey) {
    //Create initial command
    std::string passcmd ("openssl passwd -6 -salt ");
    //Specify a random salt
    passcmd.append(randomAlphaNumString(10));
    //Specify user password
    passcmd.append(" -p ");
    passcmd.append(userPassword);
    //Execute the password command
    std::string hashedPassword = runAndGetResult(passcmd);


    //Creat initial command
    std::string usercmd ("useradd -m -p ");
    //Add hashed password to command
    usercmd.append(hashedPassword);
    usercmd.append(" ");
    //Add username of new user
    usercmd.append(username);
    system(usercmd.c_str());
}

std::string runAndGetResult(std::string command){

    //Create tempfile string to be random so that the command can be runAndGetResult multiple times with low chances of overlap
    std::string tmpFile("/tmp/scalable_teaching_");
    tmpFile.append(randomAlphaNumString(4));

    //Make the command append the result to the
    command.append(" > ");
    command.append(tmpFile);
    system(command.c_str());

    std::ifstream tmpFileInputStream(tmpFile);
    std::string returnValue{std::istreambuf_iterator<char>(tmpFileInputStream), std::istreambuf_iterator<char>()};
    tmpFileInputStream.close();
    if(std::remove(tmpFile.c_str()) != 0){
        perror("Could not remove temporary file, This should have no impact and will clear itself after a reboot");
    }
    return returnValue;
}

std::string randomAlphaNumString(const int& charCount){
    //https://stackoverflow.com/questions/440133/how-do-i-create-a-random-alpha-numeric-string-in-c/440240#440240
    std::string returnString;
    static const char alphanum[] =
            "0123456789"
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            "abcdefghijklmnopqrstuvwxyz";

    srand( (unsigned) time(NULL) * getpid());

    returnString.reserve(charCount);

    for (int i = 0; i < charCount; ++i)
        returnString += alphanum[rand() % (sizeof(alphanum) - 1)];
    return returnString;
}