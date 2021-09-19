//
// Created by frederik on 3/21/21.
//
//Project headers
#include "randString.h"
//STD headers
#include <string>
#include <unistd.h>

namespace scalable{

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
}