cmake_minimum_required(VERSION 3.17)
project(SystemConfigurator)

set(CMAKE_CXX_STANDARD 17)
set(BOOST_ROOT /home/frederik/boost/1.71.0)
set(SOURCE_FILES src/main.cpp src/hostname.cpp src/hostname.h src/print.cpp src/print.h src/user.cpp src/user.h src/randString.cpp src/randString.h src/run.cpp src/run.h src/configuration.h)


find_package(Boost 1.70.0)
if(Boost_FOUND)
    message("Boost Found")
    include_directories(${Boost_INCLUDE_DIRS})
    add_executable(SystemConfigurator ${SOURCE_FILES})
    target_link_libraries(SystemConfigurator ${Boost_LIBRARIES})
endif()
