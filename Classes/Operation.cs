using System;
using System.Collections.Generic;

namespace robocopy_gui.Classes
{
  public class Operation
  {
    public bool IsEnabled { get; set; } = true;
    public bool IsArbitrary { get; set; } = false;
    public string Command { get; set; } = string.Empty;
    public string Name { get; set; }
    public string SourceFolder { get; set; }
    public string DestinationFolder { get; set; }
    public bool IsMirror { get; set; } //true -> delete extra files not present in source | false -> keep extra files (/e /xx)
                                     //mirror and move are mutually exclusive
    public bool IsMove { get; set; } = false; //true -> move files instead of copying, needs /e /xx
    public bool IsOnlyIfNewer { get; set; } = false; //ignore if target file is newer
    public bool IsUseFATTime { get; set; } = false; //useful when copying between two file systems, 2s precision
    public List<string> ExcludeFiles { get; set; }
    public List<string> ExcludeFolders { get; set; }
    public int MultiThreadCount { get; set; } = 3;
    public int RetryCount { get; set; } = 0;

    public Operation(
        string source,
        string destination,
        bool mirrorFlag = true
        )
    {
      SourceFolder = source;
      DestinationFolder = destination;
      Name = CreateName(source, destination);
      IsMirror = mirrorFlag;
      ExcludeFiles = new List<string>();
      ExcludeFolders = new List<string>();
      MultiThreadCount = 5;
      RetryCount = 5;
    }
    public Operation(
        string source,
        string destination,
        List<string> excludePatterns,
        bool excludeIsFolders,
        bool mirrorFlag = true
        )
    {
      SourceFolder = source;
      DestinationFolder = destination;
      Name = source.Substring(0, 2) + " " + source.Split("\\")[source.Split("\\").Length - 1]
          + " -> "
          + destination.Substring(0, 2) + " " + destination.Split("\\")[destination.Split("\\").Length - 1];
      IsMirror = mirrorFlag;
      if (excludeIsFolders)
      {
        ExcludeFiles = new List<string>();
        ExcludeFolders = excludePatterns;
      }
      else
      {
        ExcludeFiles = excludePatterns;
        ExcludeFolders = new List<string>();
      }
    }
    public Operation(
        string source,
        string destination,
        List<string> excludeFilePatterns,
        List<string> excludeFolderPatterns,
        bool mirrorFlag = true
        )
    {
      SourceFolder = source;
      DestinationFolder = destination;
      Name = source.Substring(0, 2) + " " + source.Split("\\")[source.Split("\\").Length - 1]
          + " -> "
          + destination.Substring(0, 2) + " " + destination.Split("\\")[destination.Split("\\").Length - 1];
      IsMirror = mirrorFlag;
      ExcludeFiles = excludeFilePatterns;
      ExcludeFolders = excludeFolderPatterns;
    }

    public Operation(string command)
    {
      string[] parts = command.Split(" ");
      int i;
      if (parts[0].ToLower() == "rem")    //detect commented lines ("REM ...")
      {
        IsEnabled = false;
        i = 2;
      }
      else
      {
        i = 1;
      }
      if (parts[i].EndsWith("\""))    //get source folder (including checking for spaces in path)
      {
        SourceFolder = parts[i];
      }
      else
      {
        while (!parts[i].EndsWith("\""))
        {
          SourceFolder += parts[i] + " ";
          i++;
        }
        SourceFolder += parts[i];
      }
      i++;
      if (parts[i].EndsWith("\""))    //get destination folder (including checking for spaces in path)
      {
        DestinationFolder = parts[i];
      }
      else
      {
        while (!parts[i].EndsWith("\""))
        {
          DestinationFolder += parts[i] + " ";
          i++;
        }
        DestinationFolder += parts[i];
      }
      i++;
      SourceFolder = SourceFolder.Replace("\"", string.Empty);
      DestinationFolder = DestinationFolder.Replace("\"", string.Empty);

      IsMirror = false;
      ExcludeFiles = new List<string>();
      ExcludeFolders = new List<string>();
      while (i < parts.Length)      //check for flags
      {
        if (parts[i].ToLower() == "/mir") { IsMirror = true; }
        if (parts[i].ToLower() == "/mov") { IsMove = true; }
        if (parts[i].ToLower() == "/xo") { IsOnlyIfNewer = true; }
        if (parts[i].ToLower() == "/fft") { IsUseFATTime = true; }
        if (parts[i].ToLower().StartsWith("/mt"))
        {
          MultiThreadCount = int.Parse(parts[i].Split(":")[1]);
        }
        if (parts[i].ToLower().StartsWith("/r"))
        {
          RetryCount = int.Parse(parts[i].Split(":")[1].ToLower());
        }
        if (parts[i].ToLower() == "/xf")    //get excluded file patterns
        {
          i++;
          while (!parts[i].StartsWith("/"))
          {
            ExcludeFiles.Add(parts[i]);
            i++;
            if (i > parts.Length - 1) { break; }
          }
          if (i > parts.Length - 1) { break; }
        }
        if (parts[i].ToLower() == "/xd")    //get excluded folder patterns
        {
          i++;
          while (!parts[i].StartsWith("/"))
          {
            ExcludeFolders.Add(parts[i]);
            i++;
            if (i > parts.Length - 1) { break; }
          }
        }
        i++;
      }
      Name = CreateName();
    }

    public Operation(bool isArbitraryCommand, bool isEnabled, string command)
    {
      if (isArbitraryCommand)
      {
        IsArbitrary = isArbitraryCommand;
        IsEnabled = isEnabled;
        Command = command;
        Name = string.Empty;
        SourceFolder = string.Empty;
        DestinationFolder = string.Empty;
        ExcludeFiles = new List<string>();
        ExcludeFolders = new List<string>();
      }
      else
      {
        // check to prevent accidental generation of wrongly-typed Operation objects
        throw new ArgumentException("Arbitrary commands must be called with isArbitraryCommand = true");
      }
    }

    public string GetCommand()
    {
      if (!IsArbitrary)
      {
        string command = "";
        if (IsEnabled)
        {
          command += "robocopy";
        }
        else
        {
          command += "REM robocopy";
        }
        command += " \"" + SourceFolder + "\"";
        command += " \"" + DestinationFolder + "\"";
        if (IsMirror && !IsMove)
        {
          command += " /mir";
        }
        else
        {
          command += " /e /xx";
        }
        if (IsMove)
        {
          command += " /mov";
        }
        if (IsOnlyIfNewer)
        {
          command += " /xo";
        }
        if (IsUseFATTime)
        {
          command += " /fft";
        }
        command += " /mt:" + MultiThreadCount + " /R:" + RetryCount;
        if (ExcludeFiles.Count > 0)
        {
          command += " /xf";
          foreach (string item in ExcludeFiles)
          {
            command += " " + item;
          }
        }
        if (ExcludeFolders.Count > 0)
        {
          command += " /xd";
          foreach (string item in ExcludeFolders)
          {
            command += " " + item;
          }
        }
        return command;
      }
      else // is arbitrary command
      {
        if (IsEnabled)
        {
          return Command;
        }
        else
        {
          return "REM " + Command;
        }
      }
    }

    public string CreateName()
    {
      if (string.IsNullOrWhiteSpace(SourceFolder) || string.IsNullOrWhiteSpace(DestinationFolder))
      {
        return string.Empty;
      }
      else
      {
        return SourceFolder.Substring(0, 2) + " " + SourceFolder.Split("\\")[SourceFolder.Split("\\").Length - 1]
        + " -> "
        + DestinationFolder.Substring(0, 2) + " " + DestinationFolder.Split("\\")[DestinationFolder.Split("\\").Length - 1];
      }
    }
    public string CreateName(string source, string destination)
    {
      if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
      {
        return string.Empty;
      }
      else
      {
        return source.Substring(0, 2) + " " + source.Split("\\")[source.Split("\\").Length - 1]
        + " -> "
        + destination.Substring(0, 2) + " " + destination.Split("\\")[destination.Split("\\").Length - 1];
      }
    }
  }
}
