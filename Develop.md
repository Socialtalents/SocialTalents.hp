# SocialTalents HP - Build instructions

## Useful links

Naming standards for unit tests
http://osherove.com/blog/2005/4/3/naming-standards-for-unit-tests.html

## Publish to NuGet

1. Download Latest Windows x86 Commandline to repository root (https://dist.nuget.org/index.html)

2. Bump Version numbers for assemblies changed (both net45 and netstandard):
X.Y.Z: 
A change in X is a breaking change
A change in Y adds functionality but is non-breaking
A change in Z represents a bug fix

3.	Run release.sh from root