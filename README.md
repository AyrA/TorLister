# TorLister

This Application gathers and lists TOR Nodes

## Difference to other Solutions

- Does not needs the TOR Binary
- Does not depends on a 3rd party Service or Website
- No hardcoded Authority List

## How it works

This Application works by directly communicating with the TOR Network.
Gathering the basic Information is rather easy and is doable with HTTP Requests.

### Authorities

The List of Authorities is grabbed from the original TOR Client Source File
[config.c](https://gitweb.torproject.org/tor.git/plain/src/or/config.c).

If a Node is added or removed it is reflected there automatically.

### Nodes

Nodes are gathered by downloading the Consensus File from a randomly chosen Authority.

## Command Line

This Tool has a minimalistic Command Line Interface.

Command: `TorLister /all | /flags | /flag flag[,...]`

### `/all`

Shows all Nodes. Add `/detail` as last Argument to get more than just the IP.

### `/flags`

Shows the list of all supported Flags.

### `/flag`

Shows Nodes that match a specified Flag.
Multiple Flags can be comma seperated (don't add any spaces).
Add `/detail` as last Argument to get more than just the IP.

## List format

List of Nodes are sorted by IP Address in ascending Order.
If `/detail` is also specified an Entry will be `Name\tIP\tTORPort\tOnlineSince\tFlags`

## Cache

This Application caches the Authority and Node List by Default.
The Authority List is cached for 24 Hours.
The Node List is cached for the Time the Authorities said it is valid,
this is usually somewhere around 4 Hours.

