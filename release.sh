echo ".net 4.5.1 version"
./nuget.exe pack SocialTalents.Hp.Events.net45/SocialTalents.Hp.Events.net45.csproj -Prop Configuration=Release
./nuget push $(pwd)/$(ls *.nupkg -t1 |  head -n 1) $(cat ../hp/nugetapi.txt) -Source https://www.nuget.org/api/v2/package
echo "--- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- "
echo "Going to Publish to nuget:"
ls SocialTalents.Hp.Events.netstandard/bin/Release/ -t1 -la |  head -n 2 | tail -n 1
read -n1 -r -p "Press any key to continue..." key
./nuget push $(pwd)/SocialTalents.Hp.Events.netstandard/bin/Release/$(ls SocialTalents.Hp.Events.netstandard/bin/Release/ -t1 |  head -n 1) $(cat ../hp/nugetapi.txt) -Source https://www.nuget.org/api/v2/package

ls SocialTalents.Hp.MongoDb.netstandard/bin/Release/ -t1 -la |  head -n 2 | tail -n 1
read -n1 -r -p "Press any key to continue..." key
./nuget push $(pwd)/SocialTalents.Hp.MongoDb.netstandard/bin/Release/$(ls SocialTalents.Hp.MongoDb.netstandard/bin/Release/ -t1 |  head -n 1) $(cat ../hp/nugetapi.txt) -Source https://www.nuget.org/api/v2/package
