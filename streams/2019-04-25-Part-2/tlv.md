# Original Implementation

The legend that is [Skape](https://twitter.com/epakskape) first implemented the TLV protocol back in 2004. For a good write up on that, and more, check out his [whitepaper](https://dev.metasploit.com/documents/meterpreter.pdf).

# Current Implementation

Single TLV:

```
     4 bytes       4 bytes
[    LENGTH    ][   TLV TYPE   ][ -------- VALUE ---------]
[                 THIS IS THE LENGTH VALUE                ]
```

TLVs are just appended together

```
[L][T][V][L][T][V][L][T][V][L][T][V][L][T][V][L][T][V]
```

Non-encrypted Packet:
```
    4 bytes         16 bytes           1 bytes       4  4    N bytes
[    XOR KEY   ][ SESSION GUID ][ ENCRYPTION FLAGS ][L][T][{  TLVs  }]
                [               THIS IS XORED                        ]
```

Encrypted Packet:
```
ENCRYPTED DATA = [L][T][{  TLVs  }]
    4 bytes         16 bytes           1 bytes          4       16 bytes        N bytes
[    XOR KEY   ][ SESSION GUID ][ ENCRYPTION FLAGS ][ LENGTH ][  Init Vec ][ ENCRYPTED DATA ]
                [               THIS IS XORED                                               ]
```

