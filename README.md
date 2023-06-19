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

##### Options

| Option | Description | Default |
| --- | --- | --- |
| -h, --host | AMQP Broker host | localhost |
| -p, --port | AMQP Broker port | 5672 |
| -u, --user | AMQP Broker user | guest |
| -P, --password | AMQP Broker password | guest |
| -q, --queue | Queue name | |
| -m, --message | Message to publish | |
| -f, --file | File to publish | |
| -e, --exchange | Exchange name | |
| -r, --routing-key | Routing key | |
|--- | --- | --- |

#### subscribe

Subscribe to a queue and print messages to stdout.

```bash
mqcat subscribe [options]
```

##### Options

| Option | Description | Default |
| --- | --- | --- |
| -h, --host | AMQP Broker host | localhost |
| -p, --port | AMQP Broker port | 5672 |
| -u, --user | AMQP Broker user | guest |
| -P, --password | AMQP Broker password | guest |
| -q, --queue | Queue name | |
| -e, --exchange | Exchange name | |
| --- | --- | --- |

## License

MIT
``` 
```

