#pragma once

#include <sstream>
#include <shellscalingapi.h>
#include <iostream>
#include <fstream>


inline void msg(const std::string& message) {
	MessageBoxA(NULL, message.c_str(), "Message", MB_OK);
}

inline void showNum(int number) {
	std::ostringstream oss;
	oss << number;
	std::string message = oss.str();
	msg(message);
}

inline std::string TResultToString(Steinberg::tresult myVal) {
	switch (myVal) {
	case 0: return "Success";
	case static_cast<Steinberg::tresult>(0x80004002): return "No interface";
	case 1: return "False";
	case static_cast<Steinberg::tresult>(0x80070057): return "Invalid argument";
	case static_cast<Steinberg::tresult>(0x80004001): return "Not implemented";
	case static_cast<Steinberg::tresult>(0x80004005): return "Internal error";
	case static_cast<Steinberg::tresult>(0x8000FFFF): return "Not initialized";
	case static_cast<Steinberg::tresult>(0x8007000E): return "Out of memory";
	default: return "Unknown value";
	}
}

//inline void AppendToLogFile(std::string textToAppend) {
//	std::ofstream logFile("output.log", std::ios_base::app);
//
//	if (logFile.is_open()) {
//		logFile << textToAppend << std::endl;
//		logFile.close();
//	}
//	else {
//		std::cerr << "Failed to open log file." << std::endl;
//	}
//
//}


inline std::string g_logFilePath;

inline void AppendToLogFile(std::string textToAppend)
{
	if (g_logFilePath.empty()) {
		std::cerr << "Log file path not set." << std::endl;
		return;
	}

	std::ofstream logFile(g_logFilePath, std::ios_base::app);
	if (logFile.is_open()) {
		logFile << textToAppend << std::endl;
		logFile.close();
	}
	else {
		std::cerr << "Failed to open log file at: " << g_logFilePath << std::endl;
	}
}