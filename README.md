[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=plkumar_mqcat&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=plkumar_mqcat)

# mqcat - a simple tool to publish and subscribe messages from AMQP Broker queues

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project src/mqcat/mqcat.csproj -- --help
```

## Usage

```bash
mqcat [command] [options]
```

### Commands

#### publish

Publish a message to a queue.

```bash
mqcat publish [options]
```

#### Options

---

| Option | Description | 
| --- | --- |
|  --ho, --host <amqp://guest:guest@127.0.0.1:5672[/vhost]> | Host uri to connect.|
|  -S, -s, --server <localhost>                             | Server name to connect. |
|  -P, --port <port>                                        | Port number to connect. [default: 5672] |
|  -V, -v, --vhost </|/vhost1>                              | vhost to connect. [default: /] |
|  -u, --user <user>                                        | Host name to connect. |
|  -p, --password <password>                                | Host name to connect. |
|  -e, --exchange <exchange> (REQUIRED)                     | Message to publish |
|  -r, --routing-key <routing-key> (REQUIRED)               | Message to publish |
|  -m, --message <message>                                  | Message to publish |
|  -f, --file, --FILE <file>                                | File path, contents of the file will be published. |
|  -?, -h, --help                                           | Show help and usage information |

#### Examples
```bash
mqcat publish -e myexchange -r myroutingkey -m "Hello World"
```

```bash
mqcat publish -e myexchange -r myroutingkey -f /path/to/file
```

```bash
mqcat publish -e myexchange -r myroutingkey -m "Hello World" -S localhost -P 5672 -V / -u guest -p guest
```

---
#### subscribe

Subscribe to a queue and print messages to stdout.

```bash
mqcat subscribe [options]
```

##### Options

| Option | Description |
| --- | --- |
| --ho, --host <host> (REQUIRED)  | Host name to connect.|
|  -S, -s, --server <localhost>   | Server name to connect.|
|  -P, --port <port>              | Port number to connect. [default: 5672]|
|  -V, -v, --vhost </|/vhost1>    | vhost to connect. [default: /]|
|  -u, --user <user>              | Host name to connect.|
|  -p, --password <password>      | Host name to connect.|
|  -q, --queue <queue> (REQUIRED) | Queue name to publish message into.|
|  -d, --durable                  | Is queue durable. [default: False]|
|  -w, --wait                     | should command wait for new messages. [default: False]|
|  -?, -h, -|-help                | Show help and usage information|


#### Examples
```bash
mqcat subscribe -q myqueue
```

```bash
mqcat subscribe -q myqueue -w
```

```bash
mqcat subscribe -q myqueue -S localhost -P 5672 -V / -u guest -p guest
```

## License

MIT


