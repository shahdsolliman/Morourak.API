@echo off
if not exist src mkdir src
move Morourak.Domain src\
move Morourak.Infrastructure src\
move Morourak.Application src\
move Morourak.API src\
echo done > result.txt
