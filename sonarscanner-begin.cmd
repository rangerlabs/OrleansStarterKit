@dotnet sonarscanner begin /k:"%SONARQUBE_PROJECT_KEY%" /o:"%SONARQUBE_ORGANIZATION%" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="%SONARQUBE_TOKEN%" /d:sonar.cs.opencover.reportsPaths="coverage.xml"