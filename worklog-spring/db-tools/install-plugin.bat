
SET PATH=%SystemRoot%\system32;%SystemRoot%;%JAVA_HOME%/bin;%KARAF_BASE%/bin;%M2_HOME%/bin

mvn install:install-file -Dfile=derby-maven-plugin-1.4.jar -DgroupId=org.jheinzel.maven -DartifactId=derby-maven-plugin -Dversion=1.4 -Dpackaging=maven-plugin -DpomFile=derby-maven-plugin-1.4.pom