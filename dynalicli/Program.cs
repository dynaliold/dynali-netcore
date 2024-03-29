﻿using Dynali;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace dynalicli
{
    class Program
    {
        struct Hostname
        {
            public string Username;
            public string Password;
        }
        static Dictionary<string, Hostname> loadHostnames(string filepath)
        {
            Dictionary<string, Hostname> hostnames = new Dictionary<string, Hostname>();

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("File does not exist.", filepath);
            }

            StreamReader reader = new StreamReader(filepath);
            while (reader.Peek() != -1)
            {
                string[] lineParts = reader.ReadLine().Split(',');
                if (lineParts.Length == 3)
                {
                    hostnames[lineParts[0]] = new Hostname() { Password = lineParts[2], Username = lineParts[1] };
                }
            }
            reader.Close();
        }

        static void Main(string[] args)
        {
            string[] commands = { "ip", "install", "add", "remove", "status", "update", "update-all", "list", "changepassword" };
            if (args.Length < 2 || commands.Contains(args[1]))
            {
                Console.WriteLine("Please provide one of the following commands as the first argument: " + String.Join(',', commands));
                Environment.Exit(-1);
            }

            DynaliClient client = new DynaliClient();
            Dictionary<string, Hostname> hostnames = loadHostnames("dynali.csv");



            /*
             *

$client = new DynaliClient();
$hostnames = [];
if (file_exists('dynali.csv')) {
    if (!is_readable(('dynali.csv'))) {
        echo '`dynali.csv` file exists, but is not readable. Exitting...' . PHP_EOL;
        exit(-2);
    }

    $file = fopen('dynali.csv', 'r');
    while ($csvRow = fgetcsv($file)) {
        $hostnames[$csvRow[0]] = ['u' => $csvRow[1], 'p' => $csvRow[2]];
    }
    fclose($file);
}

function update($hostname, $username, $password)
{
    global $client;
    echo 'Updating `' . $hostname . '`: ';
    try {
        $response = $client->update($hostname, $username, $password);
        if ($response === true) {
            echo '[OK]' . PHP_EOL;
        }
    } catch (Exception $e) {
        echo $e->getMessage() . PHP_EOL;
    }
}


switch ($command) {
    case 'ip':
        echo $client->myIp() . PHP_EOL;
        break;
    case 'install':
        echo "Please add to Crontab:" . PHP_EOL;
        echo '* * * * * cd ' . dirname(__FILE__) . ' && ./dynali update-all 2>&1 > dynali.log' . PHP_EOL;
        break;
    case 'status':
        $hostname = $argvReader->getArgument(1);
        if ($hostname === null) {
            echo "Please provide hostname as the second argument." . PHP_EOL;
            exit(-11);
        }

        if (!isset($hostnames[$hostname])) {
            echo "Provided hostname is not defined. Please add it first using `add` method." . PHP_EOL;
        }

        $password = $hostnames[$hostname]['p'];
        $username = $hostnames[$hostname]['u'];
        try {
            echo 'Testing hostname...' . PHP_EOL;
            $response = $client->status($hostname, $username, $password);
            if ($response instanceof DynaliStatus) {
                $hostnames[$hostname] = [
                    'u' => $username,
                    'p' => $password
                ];
            }
            echo 'Hostname: ' . $hostname . PHP_EOL;
            echo 'Status: ' . $response->getStatus() . PHP_EOL;
            echo 'IP: ' . $response->getIp() . PHP_EOL;
            echo 'Status message: ' . $response->getStatusMessage() . PHP_EOL;
            echo 'Expiry date: ' . $response->getExpiryDate()->format('Y-m-d H:i:s') . PHP_EOL;
            echo 'Creation date: ' . $response->getCreationDate()->format('Y-m-d H:i:s') . PHP_EOL;
        } catch (Exception $e) {
            echo 'Error while testing hostname:' . PHP_EOL;
            echo $e->getMessage() . PHP_EOL;
            exit(-7);
        }

        break;
    case 'update-all':
        if (file_exists('job.run')) {
            echo 'Exitting: previous job still running...' . PHP_EOL;
            exit(-10);
        }

        file_put_contents('job.run', date('u'));
        echo 'Testing ' . count($hostnames) . ' hostnames...' . PHP_EOL;
        foreach ($hostnames as $hostname => $data) {
            update($hostname, $data['u'], $data['p']);
        }

        unlink('job.run');
        break;
    case 'changepassword':
        $hostname = $argvReader->getArgument(1);
        $username = $argvReader->getArgument(2);
        $password = $argvReader->getArgument(3);
        $newpassword = $argvReader->getArgument(4);

        if ($hostname === null || $username === null || $password === null || $newpassword === null) {
            echo "Usage:" . PHP_EOL;
            echo "./dynali changepassword <hostname> <username> <password> <new password>" . PHP_EOL;
            exit(-9);
        }

        echo 'Changing password for `' . $hostname . '`: ';
        try {
            $response = $client->changePassword($hostname, $username, $password, $newpassword);
            if ($response === true) {
                echo '[OK]' . PHP_EOL;
            }
        } catch (Exception $e) {
            echo $e->getMessage() . PHP_EOL;
        }
        break;
    case 'update':
        $hostname = $argvReader->getArgument(1);
        if ($hostname === null) {
            echo "Please provide hostname as the second argument." . PHP_EOL;
        }

        if (!isset($hostnames[$hostname])) {
            echo "Provided hostname is not defined. Please add it first using `add` method." . PHP_EOL;
        }

        $password = $hostnames[$hostname]['p'];
        $username = $hostnames[$hostname]['u'];

        update($hostname, $data['u'], $data['p']);
        break;
    case 'list':
        echo 'Listing ' . count($hostnames) . ' hostnames.' . PHP_EOL;
        foreach ($hostnames as $hostname => $data) {
            echo $hostname . ' (' . $data['u'] . ')' . PHP_EOL;
        }
        break;
    case 'remove':
        $hostname = $argvReader->getArgument(1);
        if ($hostname === null) {
            echo "Please provide hostname as the second argument." . PHP_EOL;
        }

        if (!isset($hostnames[$hostname])) {
            echo "Provided hostname is not defined. Please add it first using `add` method." . PHP_EOL;
        }

        unset($hostnames[$hostname]);
        if (!is_writable(('dynali.csv'))) {
            echo '`dynali.csv` file exists, but is not writable. Exitting...' . PHP_EOL;
            exit(-3);
        }

        $file = fopen('dynali.csv', 'w');
        foreach ($hostnames as $hostname => $data) {
            fputcsv($file, [$hostname, $data['u'], $data['p']]);
        }
        fclose($file);
        break;
    case 'add':
        $hostname = $argvReader->getArgument(1);
        $username = $argvReader->getArgument(2);
        $password = $argvReader->getArgument(3);

        if ($hostname === null || $username === null || $password === null) {
            echo "Usage:" . PHP_EOL;
            echo "./dynali add <hostname> <username> <password>" . PHP_EOL;
            exit(-4);
        }

        if (isset($hostnames[$hostname])) {
            echo 'Provided hostname is already defined. Please remove it first.' . PHP_EOL;
            exit(-5);
        }

        try {
            echo 'Testing hostname...' . PHP_EOL;
            $response = $client->status($hostname, $username, $password);
            if ($response instanceof DynaliStatus) {
                $hostnames[$hostname] = [
                    'u' => $username,
                    'p' => $password
                ];
            }
        } catch (Exception $e) {
            echo 'Error while adding hostname:' . PHP_EOL;
            echo $e->getMessage() . PHP_EOL;
            exit(-6);
        }

        $file = fopen('dynali.csv', 'w');
        foreach ($hostnames as $hostname => $data) {
            fputcsv($file, [$hostname, $data['u'], $data['p']]);
        }
        fclose($file);
        break;
}
*/
        }
    }
}
