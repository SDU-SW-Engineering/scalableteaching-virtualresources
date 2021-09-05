# Json config for configuring a machine 
```json
{
  "hostname": "NewHostName",
  "groups": ["group1", "group2", "systemGroup"],
  "systemUser": {
    "username": "scalable_teaching_system_user",
    "userPassword": "SomeRandomPassword",
    "userPublicKey": "ThePublicKey",
    "groups": ["systemGroup"]
  },
  "users": [
    {
      "username": "someAssignedUser",
      "userPassword": "systemAssignedPassword",
      "userPublicKey": "systemAssignedPublicKey",
      "groups": ["student"]
    },
    {
      "username": "someOtherAssignedUser",
      "userPassword": "systemAssignedPassword",
      "userPublicKey": "systemAssignedPublicKey",
      "groups": ["student"]
    }
  ],
  "aptPPA": [],
  "aptPackages":["git, vim, gedit"]

}
```
* All fields must be populated
* All array must exists even if empty
