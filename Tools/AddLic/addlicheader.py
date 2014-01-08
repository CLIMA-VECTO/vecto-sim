# Copyright 2014 European Union.
# Licensed under the EUPL (the 'Licence');
#
# * You may not use this work except in compliance with the Licence.
# * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
# * Unless required by applicable law or agreed to in writing,
#   software distributed under the Licence is distributed on an "AS IS" basis,
#   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#
# See the LICENSE.txt for the specific language governing permissions and limitations.

import os
import os.path
import glob
import sys

def addLicenseHeader(path, lic, no_write=False):
    for fname in glob.glob(path):

        print('%s: ' % fname, end='')
        txt = open(fname).read()


        ## Remove BOM
        #
        if txt.startswith(u'\ufeff'):
            print('BOM removed, ', end='')
            txt = txt[1:]

        line1 = lic.split('\n')[0]
        if txt.startswith(line1):
            print('already licensed! Skipping it.')
        else:
            if no_write:
                print('NO WRITE')
            else:
                with (open(fname, 'w', newline = '\r\n')) as fd:
                    fd.write(lic + txt)
                print('licensed ok.')


if __name__ == '__main__':
    licfname = './lic_vb.txt'
    lic = open(licfname, ).read()


    mydir = os.path.dirname(__file__)
    paths = [os.path.join(mydir, '..', '..', d) for d  in [
        'VECTO/*.vb',
        'VECTO/MODcalc/*.vb',
        'VECTO/GUI/*.vb',
        'VECTO/Input Files/*.vb',
        'VECTO//*.vb',
        'VECTO//*.vb',
        'VECTO/File Browser/*.vb',
    ]]
    for path in paths:
        addLicenseHeader(path, lic=lic, no_write = False)