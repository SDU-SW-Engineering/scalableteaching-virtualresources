#include <iostream>
#include <fstream>
#include <array>
#define HOSTS_FILE "/etc/hosts"
#define HOSTNAME_FILE "/etc/hostname"

void println(std::string output);
bool validateHostname(char *hostname);

int main(int argc, char **argv) {
    //Validate that input has been given
    if(argc < 2){
        println("The program takes the new hostname as first argument");
        return 1;
    }
    if(!validateHostname(argv[1])) return 1;//validateHostname prints its own debug messages

    //TODO: Change hostname in hostname file

    //TODO: Change hostname in hosts file (Look at the name stored in hostname file and search in this one)

    return 0;
}

void println(std::string output){
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
}