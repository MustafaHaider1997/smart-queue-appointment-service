# Base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5243

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy and restore solution
COPY ["AppointmentService.sln", "./"]
COPY ["Appointment.API/Appointment.API.csproj", "Appointment.API/"]
COPY ["Appointment.Application/Appointment.Application.csproj", "Appointment.Application/"]
COPY ["Appointment.Domain/Appointment.Domain.csproj", "Appointment.Domain/"]
COPY ["Appointment.Infrastructure/Appointment.Infrastructure.csproj", "Appointment.Infrastructure/"]

RUN dotnet restore "Appointment.API/Appointment.API.csproj"

# Copy the rest and build
COPY . .
WORKDIR "/src/Appointment.API"
RUN dotnet build "Appointment.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Appointment.API.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Appointment.API.dll"]

