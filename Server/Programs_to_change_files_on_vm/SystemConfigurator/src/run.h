//
// Created by frederik on 3/21/21.
//

#ifndef SYSTEMCONFIGURATOR_RUN_H
#define SYSTEMCONFIGURATOR_RUN_H
#include <string>

namespace scalable {
    std::string runAndGetResult(const char *command, bool printCommand, bool onlyFirstLine = true);
    void run(const char *command, bool printCommand=true);
}
#endif //SYSTEMCONFIGURATOR_RUN_H
