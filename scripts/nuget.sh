set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder


dotnet build ./src/KafkaMessageBus/Aix.KafkaMessageBus.csproj -c Release

dotnet pack ./src/KafkaMessageBus/Aix.KafkaMessageBus.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/Aix.KafkaMessageBus.*.nupkg -k $PRIVATE_NUGET_KEY -s http://192.168.102.34:8081/repository/nuget-hosted
