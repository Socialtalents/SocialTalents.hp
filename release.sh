echo "Going to Publish to nuget:"
ls SocialTalents.Hp.Events/bin/Release/ -t1 -la |  head -n 2 | tail -n 1
read -n1 -r -p "Press any key to continue..." key
./nuget push $(pwd)/SocialTalents.Hp.Events/bin/Release/$(ls SocialTalents.Hp.Events/bin/Release/ -t1 |  head -n 1) $(cat ../hp/nugetapi.txt) -Source https://www.nuget.org/api/v2/package