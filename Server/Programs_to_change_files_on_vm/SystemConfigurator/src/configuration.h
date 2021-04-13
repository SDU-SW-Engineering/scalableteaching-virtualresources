//
// Created by frederik on 3/24/21.
//

#ifndef SYSTEMCONFIGURATOR_CONFIGURATION_H
#define SYSTEMCONFIGURATOR_CONFIGURATION_H

#include <json.hpp>
#include <vector>
#include <string>

using nlohmann::json;
namespace scalable::configuration::user {
    struct user {
        std::string username;
        std::string userPassword;
        std::string userPublicKey;
        std::vector<std::string> groups;
    };

    void to_json(json &j, const user &u) {
        j = json{
                {"username",      u.username},
                {"userPassword",  u.userPassword},
                {"userPublicKey", u.userPublicKey},
                {"groups",        u.groups}
        };
    }

    void from_json(const json &j, user &u) {
        j.at("username").get_to(u.username);
        j.at("userPassword").get_to(u.userPassword);
        j.at("userPublicKey").get_to(u.userPublicKey);
        j.at("groups").get_to(u.groups);
    }
}


namespace scalable::configuration {
    struct configuration {
        std::string hostname;
        std::vector<std::string> groups;
        std::vector<scalable::configuration::user::user> users;
        std::vector<std::string> aptPPA;
        std::vector<std::string> aptPackages;
    };

    void to_json(json &j, const configuration &c) {
        j = json{
                {"hostname",    c.hostname},
                {"groups",      c.groups},
                {"users",       c.users},
                {"aptPPA",      c.aptPPA},
                {"aptPackages", c.aptPackages}};
    }

    void from_json(const json &j, configuration &c) {
        j.at("hostname").get_to(c.hostname);
        j.at("groups").get_to(c.groups);
        j.at("users").get_to(c.users);
        j.at("aptPPA").get_to(c.aptPPA);
        j.at("aptPackages").get_to(c.aptPackages);
    }
}
#endif //SYSTEMCONFIGURATOR_CONFIGURATION_H
