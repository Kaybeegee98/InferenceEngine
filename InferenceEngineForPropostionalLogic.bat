@echo off
title COS30019 Introduction to Artificial Intelligence Assignment 2
set /p path=Please Enter Filepath for .txt file of the Environment:
set /p string=Please Enter the type of Search (TT/TruthTable, FC/ForwardChaining, BC/BackwardChaining):
"%~dp0InferenceEngine\InferenceEngine\bin\Debug\net6.0\InferenceEngine.exe" "%path%" "%string%" > CON
pause