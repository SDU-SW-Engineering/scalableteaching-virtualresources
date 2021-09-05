//
// Created by frederik on 3/21/21.
//

#ifndef SYSTEMCONFIGURATOR_USER_H
#define SYSTEMCONFIGURATOR_USER_H
#include <string>

namespace scalable{
    int addUser(const std::string& username, const std::string& userPassword, const std::string &userPublicKey);
    int addGroup(const std::string& groupName);
    int assignToGroup(const std::string &username, const std::string &groupname);
    bool userExists(const std::string& username);
    bool groupExists(const std::string& groupname);
}

#endif //SYSTEMCONFIGURATOR_USER_H
