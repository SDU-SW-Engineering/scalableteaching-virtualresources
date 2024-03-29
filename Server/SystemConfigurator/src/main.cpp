
//Project headers
#include "hostname.h"
#include "print.h"
#include "user.h"
#include "configuration.h"
#include "run.h"
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
int parseConfiguration(scalable::configuration::configuration& config);

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
        return 1;
    }
    auto config = parseConfigurationToConfigurationStruct(argv[1]);
    return parseConfiguration(config);
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

    //Dont update hostname
//    if(!validateHostname(config.hostname)){
//        errorCode += 1;
//    }
//    else {
//        //Update Hostname
//        updateHostname(config.hostname);
//    }


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
    run("apt update");

    bool addedRepo = false;
    //Add PPA's
    for(const std::string& ppa : config.aptPPA){
        addedRepo = true;
        std::string ppaCommandString{"add-apt-repository -y "};
        ppaCommandString.append(ppa);
        run(ppaCommandString.c_str());
    }
    //Update any included ppa
    if(addedRepo) run("apt update");

    //Install all updates
    run("apt upgrade -y");

    //Install apt packages
    for(const std::string& package : config.aptPackages){
        std::string aptInstallString{"apt install -y "};
        aptInstallString.append(package);
        run(aptInstallString.c_str());
    }

    //Apt Cleanup
    run("sudo apt autoremove -y");

    return errorCode;
}

scalable::configuration::configuration
parseConfigurationToConfigurationStruct(const char* configFile){
    std::ifstream inputFileStream(configFile);
    if(inputFileStream.bad()){
        perror("Error occurred upon reading the configfile");
    }
    std::string fileText {std::istreambuf_iterator<char>(inputFileStream), std::istreambuf_iterator<char>()};
    if(inputFileStream.bad()){
        perror("Error occurred upon reading the configfile");
    }
    inputFileStream.close();
    json parsedJson = json::parse(fileText);

    scalable::configuration::configuration config;
    try {
        config = parsedJson;
    }
    catch (std::exception& e){
        perror(e.what());
    }
    return config;
}





