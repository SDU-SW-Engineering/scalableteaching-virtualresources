binaryLocation=´../cmake-build-release/SystemConfigurator´
version=`echo version`
old_release_date=`echo oldReleaseDate`

all: st-system-configurator_1.0-1.deb
	
st-system-configurator_1.0-1.deb: folder-structure
	dpkg-deb --build st-system-configurator_1.0-1

binary: folder-structure
	cp ../cmake-build-release/SystemConfigurator ./st-system-configurator

folder-structure: versioning


versioning:
	ifeq(`stat -c %y `)
	
