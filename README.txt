Author: Ryan Farr
Date: May 7th, 2016
File: README.txt

Within this repo are the PPSAT solver itself as well as its testing software. PPSAT is a Partially Parallelized Satisfiability Solver that was created for my Rigorous Systems Design (CS 5110/6110) course at the University of Utah. The purpose of the project was to build a simple SAT solver, build testing software for it, and investigate basic forms of parallelization. Only two basic forms of parallelization were used, one where we simply run the main solving method in multiple threads (which has no guarantees that two threads won't go down similar paths) and the other which simply uses multithreading to go down both
paths of the decision tree on Decide.

To run the solver you can download the source code and build the program yourself, or alternatively grab the executable file and use the command "PPSAT.exe path" where path is replaced with the filepath to a CNF file that represents the boolean expression you wish to determine the satisfiability of. The executable should inform the user that the expression is either SAT or UNSAT, and if it's SAT then it will print out a model that satisfies the expression.

To run the testing software you can download the testing project. You'll have to have PPSAT.exe within your PATH environment variable along with minisat.exe. They're both java programs, so you can use any java IDE to run them or simply use "javac file.java" to compile and "java name" to run that program.