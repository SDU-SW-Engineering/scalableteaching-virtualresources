//
// Created by frederik on 3/21/21.
//



//Project headers
#include "user.h"
#include "randString.h"
#include "run.h"
//Third party headers
//STD headers
#include <string>
#include <fstream>

namespace scalable{
    ///
    /// \param username
    /// \param userPassword
    /// \param userPublicKey
    /// \return
    int addUser(const std::string& username, const std::string& userPassword, const std::string &userPublicKey) {
        if(userExists(username)) return 1;
        //Create initial command
        std::string passcmd {"openssl passwd -6 -salt "};
        //Specify a random salt
        passcmd.append(randomAlphaNumString(10));
        //Specify user password
        passcmd.append(" -p ");
        passcmd.append(userPassword);
        //Execute the password command
        std::string hashedPassword = scalable::runAndGetResult(passcmd.c_str());


        //Creat initial command
        std::string usercmd {"useradd -m -p "};
        //Add hashed password to command
        usercmd.append(hashedPassword);
        usercmd.append(" ");
        //Add username of new user
        usercmd.append(username);
        system(usercmd.c_str());
        return 0;
    }

    ///
    /// \param groupName
    /// \return 1 if group exists or 0 if group did not exist
    int addGroup(const std::string& groupName){
        if(groupExists(groupName)) return 1;
        //Create initial command
        std::string groupcmd  {"groupadd "};
        groupcmd.append(groupName);
        system(groupcmd.c_str());
        return 0;
    }

    ///
    /// Adds a group assignment to a user
    /// \param username name of user
    /// \param groupname name of group
    /// \return 0 if group and user exists, 1 if user is missing
    int assignToGroup(const std::string &username, const std::string &groupname) {
        if(!groupExists(groupname)){
            addGroup(groupname);
        }
        if(userExists(username)){
            std::string assigncmd{"usermod -a "};
            assigncmd.append(username);
            assigncmd.append(" -G ");
            assigncmd.append(groupname);
            system(assigncmd.c_str());
            return 0;
        }else{
            return 1;
        }
    }

    ///
    /// \param groupname
    /// \return
    bool groupExists(const std::string& groupname){
        std::ifstream groupsFile ("/etc/groups");
        std::string lineBuffer;
        while(std::getline(groupsFile, lineBuffer)){
            int index = lineBuffer.find(groupname);
            if(index == 0 && lineBuffer.at(index + groupname.length())  == ':' ){
                groupsFile.close();
                return true; //Return true if any line starts with the groupname
            }
        }
        groupsFile.close();
        return false;
    }

    ///
    /// \param username
    /// \return
    bool userExists(const std::string& username){
        std::ifstream groupsFile ("/etc/passwd");
        std::string lineBuffer;
        while(std::getline(groupsFile, lineBuffer)){
            int index = lineBuffer.find(username);
            if(index == 0 && lineBuffer.at(index + username.length())  == ':' ){
                groupsFile.close();
                return true; //Return true if any line starts with the username
            }
        }
        groupsFile.close();
        return false;
    }

}