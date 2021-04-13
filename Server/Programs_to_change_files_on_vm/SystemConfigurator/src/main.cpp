
//Project headers
#include "hostname.h"
#include "print.h"
#include "user.h"
#include "configuration.h"
//Third party headers
#include <json.hpp>
//STD headers
#include <fstream>
#include <iostream>
#include <string>
#include <vector>


using namespace scalable;
using nlohmann::json;

scalable::configuration::configuration parseConfigurationToConfigurationStruct(const char* configFile);
int parseConfiguration(json& config);

///
/// Errors:
/// 0: No error
/// 1: Invalid hostname
/// 2: A group to be added already existed;
/// 4: System User already exists
/// \param argc Argument count
/// \param argv Arguments
/// \return error indicator int as series of flags
int main(int argc, char **argv) {
    if(argc < 2){
        println("The configurator requires a path to a valid configuration file");
    }
    json jsonData{ parseConfigurationToConfigurationStruct(argv[1])};
    return parseConfiguration(jsonData);
}

///
/// Errors:
/// 1: Invalid hostname
/// 2: A group to be added already existed;
/// Configures machine based on the given configuration
/// \param config
/// \return indicator of success 0 for no error
int parseConfiguration(scalable::configuration::configuration& config){
    int errorCode{0};

    if(!validateHostname(config.hostname)){
        errorCode += 1;
    }
    else {
        //Update Hostname
        updateHostname(config.hostname);
    }
    //Add groups
    int groupAdd{};
    for(const std::string& string : config.groups) {
        groupAdd += addGroup(string);
    }
    if (groupAdd) errorCode += 2;

    //Create individual users
    for(const scalable::configuration::user::user& user : config.users){
        std::string userUsername{user.username};
        addUser(userUsername,
                user.userPassword,
                user.userPublicKey);
        for(const std::string& group : user.groups){
            assignToGroup(userUsername, group);
        }
    }

    //Update apt
    system("apt update");

    //Add PPA's
    for(const std::string& ppa : config.aptPPA){
        std::string ppaCommandString{"add-apt-repository -y "};
        ppaCommandString.append(ppa);
        system(ppaCommandString.c_str());
    }
    //Update to include PPA's
    system("apt update");

    //Install apt packages
    for(const std::string& package : config.aptPackages){
        std::string aptInstallString{"apt install -y "};
        aptInstallString.append(package);
        system(aptInstallString.c_str());
    }

    //Install all updates
    system("apt upgrade -y");

    return errorCode;
}

scalable::configuration::configuration
parseConfigurationToConfigurationStruct(const char* configFile){
    std::ifstream inputFileStream(configFile);
    if(inputFileStream.bad()){
        perror("Error occurred upon reading the configfile");
    }
    std::string fileText {std::istreambuf_iterator<char>(inputFileStream), std::istreambuf_iterator<char>()};
    inputFileStream.close();
    scalable::configuration::configuration config = json::parse(fileText);
    return config;
}





