//
// Created by frederik on 3/21/21.
//

#ifndef SYSTEMCONFIGURATOR_HOSTNAME_H
#define SYSTEMCONFIGURATOR_HOSTNAME_H

#include <string>

namespace scalable{
    bool validateHostname(const std::__cxx11::basic_string<char> &hostname);
    int updateHostname(const std::string &newHostName);
}

#endif //SYSTEMCONFIGURATOR_HOSTNAME_H
