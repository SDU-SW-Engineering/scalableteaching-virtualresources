cmake_minimum_required(VERSION 3.16)
project(UpdateHostName)

set(CMAKE_CXX_STANDARD 14)
set(BOOST_ROOT /usr/local/boost/boost_1_75_0)
set(Boost_INCLUDE_DIR /usr/local/boost/boost_1_75_0)

set(SOURCE_FILES main.cpp)

find_package(Boost 1.75.0)
include_directories(${Boost_INCLUDE_DIRS})

if (Boost_FOUND)
    include_directories(${Boost_INCLUDE_DIRS})
    add_executable(UpdateHostName ${SOURCE_FILES})
    target_link_libraries(UpdateHostName ${Boost_LIBRARIES} -missing_libs)
endif()
if(NOT Boost_FOUND)
    message(FATAL_ERROR "Could Not Find BOOST D:")
endif()
