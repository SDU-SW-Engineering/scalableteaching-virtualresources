cmake_minimum_required(VERSION 3.17)
project(SystemConfigurator)

set(CMAKE_CXX_STANDARD 20)
set(BOOST_ROOT /usr/local/boost/boost_1_75_0)
set(Boost_INCLUDE_DIR /usr/local/boost/boost_1_75_0)

set(SOURCE_FILES main.cpp)
find_package(Boost 1.75.0)

add_executable(SystemConfigurator ${SOURCE_FILES})
if (Boost_FOUND)
    include_directories(${Boost_INCLUDE_DIRS})
    target_link_libraries(SystemConfigurator ${Boost_LIBRARIES} -missing_libs)
endif()
if(NOT Boost_FOUND)
    message(FATAL_ERROR "Could Not Find BOOST D:")
endif()

