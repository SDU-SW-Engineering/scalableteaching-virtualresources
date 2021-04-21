//
// Created by frederik on 3/21/21.
//
//Project headers
#include "randString.h"
#include "run.h"
//STD headers
#include <fstream>
#include <string>
#include <iostream>


namespace scalable {
    ///
    /// \param command
    /// \param printCommand default true: prints the command
    /// \return The output of the command
    std::string runAndGetResult(const char *command, bool printCommand, bool onlyFirstLine) {
        if (printCommand) std::cout << "Run Command: " << command << std::endl;
        //Create tempfile string to be random so that the command can be runAndGetResult multiple times with low chances of overlap
        std::string tmpFile("/tmp/scalable_teaching_");
        tmpFile.append(randomAlphaNumString(4));

        //Make the command append the result to the
        std::string cmd{command};
        cmd.append(" > ");
        cmd.append(tmpFile);
        auto _ = system(cmd.c_str());

        std::ifstream tmpFileInputStream(tmpFile);
        std::string returnValue{std::istreambuf_iterator<char>(tmpFileInputStream), std::istreambuf_iterator<char>()};
        tmpFileInputStream.close();
        if (std::remove(tmpFile.c_str()) != 0) {
            perror("Could not remove temporary file, This should have no impact and will clear itself after a reboot");
        }
        returnValue = returnValue.substr(0,returnValue.find('\n'));
        return returnValue;
    }

    void run(const char *command, bool printCommand) {
        if (printCommand) std::cout << "Run Command: " << command << std::endl;
        auto _ = system(command);
    }
}