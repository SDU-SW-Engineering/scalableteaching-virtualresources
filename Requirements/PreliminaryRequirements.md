# Requirements
## Need to have
* Account creation for system administrators
	* Account escalation to educator level.
* Account system connected to sdu o-auth.
* Interface to see machines you have credentials for.
	* Present machines in a sorted fashion based on class as categories and then names.
	* Allow the user to download a zip of ssh_configs id_rsa files for ssh connections.
		* Allow the user to download a fully compiled zip for all machines that the user has credentials for.
		* Allow for the display of the ssh config in browser so that the user can copy it manually.
	* Allow user to start, stop, and reset machine 
* Interface to manage and create machines - Accessible by educator level and above accounts.
	* Creating courses to group machines for naming and sorting on all views.
	* Import lists of users to have "identical machines" in the form of csv files.
	* Import lists of groups containing users to have machines assigned to groups.
	* Assign multiple machines pr group.
		* Specify single user pr group or individual users on each machine.
	* Specify user access levels.
		* Specify user groups.
		* Specify user sudo rights and restrictions.
	* Specify apt packages to be added to the system.
		* Allow specification of (repos), names and (versions).
	* Specify naming for machine.
	* Allow Parameter and counter based naming.
	* Allow for the export of machine configurations for individual machines of for an entire class.
	* Provide a list of bash scripts to be run once on the machine.
	* Provide files that are supposed to be on every machine.
	* Provide wgettable links for deb packages
	* Provide wgettable links to files to be downloaded, and a location for their download.
	* Allow for scheduling virtual machines for deletion.
		* Allow people to cancle machine deletion.
		* Default of 8 months of lifetime for a machine unless extended tbd.
	* Selection of x11 support.
	* Later expansion of specific machines storage.
* Educator has a user, key, and password on the machines created.
* System has user, key, and password on the machines created.
* Access for system administrator to all machines.
* Access for system administrator to all machines without ssh.


## Nice to have
* Upload of customised image for replication.
* Ability to reset ssh configuration thru script on machine.
* Add and remove users from existing machines.
* Creating a machine and configuring it in the system, and the having it replicated with users and so on.
* Packet routing or proxy like system to allow access from outside the system. ( May not be possible ).
* Secondary storage that expands or overwrites to prevent servers from crashing.
* Machines for exam use that are very limited in content.
* Pre configured systems
	* Database for class, group or individual use.
	* Docker machine for class, group or individual use.
* Windows machines.
* Script interface for the system allowing for the same functionality as the web interface.
