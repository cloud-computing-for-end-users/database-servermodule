FROM mcr.microsoft.com/mssql/server:2017-latest as base



#### COPY FROM THE BASE IMAGE (sotred in docker folder in the hello-world repository) ###

# start nessesary for install of dot net core 2.2
RUN apt update
RUN apt install wget -y	
RUN apt install software-properties-common -y


#installing .Net Core 2.2
RUN add-apt-repository universe
RUN apt-get update
RUN apt-get install apt-transport-https -y
RUN apt-get update
RUN apt-get install aspnetcore-runtime-2.2=2.2.0-1 -y



#installing vim for ease of debugging in interactive mode
RUN apt install vim -y

#install ip command simular to ifconfig but better
RUN apt install iproute2 -y

#installing ping command
RUN apt install iputils-ping -y

#### END OF COPY ####

###### setting up sqlserver with database and table/data #####
## setting envioremental variables needed for sql server
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Pwd12345!

# copy sql files
COPY /SQL/ /data/database-servermodule/SQL/

#execute sql files on the sqlserver database
# https://www.youtube.com/watch?v=Yj69cWEG_Yo&t=615s at:  30 min
RUN /opt/mssql/bin/sqlservr --accept-eula & sleep 10 \
	&& /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Pwd12345!" -i /data/database-servermodule/SQL/0_001_Drop_Database.sql /data/database-servermodule/SQL/1_001_Initialize_Database.sql /data/database-servermodule/SQL/5_001_User_Table.sql /data/database-servermodule/SQL/7_001_User_Insert.sql

####### end of sqlserver setup ######

#RUN apt install parallel -y #so that sqlservr and database-servermodule can run both at the same time

# copy application 
COPY /src/database-servermodule/bin/Debug/netcoreapp2.2/publish/ /data/database-servermodule/

ENTRYPOINT ["dotnet", "/data/database-servermodule/database-servermodule.dll"]



CMD ["scp:5542", "rcp:5522", "rrp:5523", "isLocal:false","rip:172.17.0.2"]