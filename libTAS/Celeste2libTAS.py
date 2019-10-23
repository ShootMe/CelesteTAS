#!/usr/bin/env python
# This script converts tas files from Celeste TAS tool
# (https://github.com/ShootMe/CelesteTAS/) to libTAS input file.
# Just run ./Celeste2libTAS path/to/tasfile.tas

import re
import sys
import os
import glob
import math

input_file = open(sys.argv[1], 'r')
output_ltm = open(os.path.splitext(sys.argv[1])[0]+'.ltm' , 'w')

regex_input = re.compile(r'[\s]*([\d]*)((?:,(?:[RLUDJKXCGSQNFO]|[\d]*))*)')
regex_comment = re.compile(r'[\s]*(#|[\s]*$)')

def GetLine(labelOrLineNumber, file):
    try:
        return int(labelOrLineNumber)
    except ValueError:
        curLine = 0
        for line in file:
            curLine += 1
            if (line == (f'#{labelOrLineNumber}\n')):
                return curLine
        return float('inf')

def GetReadData(line):
    index = line.find(',')
    if index > 0:
        filePath = line[0:index]
    else:
        filePath = line[0:-1]

    filePath = os.path.dirname(sys.argv[1]) + '/' + filePath
    # Check if full filename was used, get file if it wasn't
    if (not os.path.exists(filePath)):
        files = [f for f in glob.glob(f'{filePath}*.tas')]
        if files == []:
            return None, None, None
        filePath = str(files[0])
    file = open(filePath, 'r')
    skipLines = 0
    lineLen = float('inf')

    # Check how many line numbers were given and convert any labels to lines
    if (index > 0):
        indexLen =  line.find(',', index + 1)
        if (indexLen > 0):
            startLine = line[index + 1: indexLen]
            endLine = line[indexLen + 1:-1]
            skipLines = GetLine(startLine, file)
            lineLen = skipLines + GetLine(endLine, file)
        else:
            startLine = line[index + 1:-1]
            skipLines = GetLine(startLine, file)
    if skipLines == None:
        skipLines = 0
    return file, skipLines, lineLen


def ExportFile(file, startLine = 0, endLine = float('inf')):
    file.seek(0)
    curLine = 0
    print(file, startLine, endLine)

    for line in file:
        curLine += 1
        if curLine <= startLine:
            continue
        if curLine > endLine:
            break

        if regex_comment.match(line):
            continue
        if line.lower().startswith('read'):
            readPath, start, end = GetReadData(line[5:])
            if readPath != None:
                ExportFile(readPath, start, end)
            continue
        if line.lower().startswith('add'):
            line = line[3:]

        match = regex_input.match(line)
        if match:
            output_keys = ""

            button_order   = "ABXYbgs()[]udlr"
            output_buttons = list("...............")

            button_mapping = "JXCK..S...GUDLR"
            output_axes = "0:0"

            is_axis = False
            for single_input in match.group(2).split(',')[1:]:

                if is_axis:
                    if single_input == '':
                        angle = 0
                    else:
                        angle = int(single_input)

                    # Compute coordinates of the left analog stick to match the
                    # requested angle. Use the max amplitude to get precise values.
                    # We must also compensate for the deadzone which is 0.239532471f
                    rad_angle = math.radians(angle)
                    deadzone = 0.239532471
                    float_x = math.copysign(math.fabs(math.sin(rad_angle))*(1-deadzone)+deadzone, math.sin(rad_angle))
                    float_y = math.copysign(math.fabs(math.cos(rad_angle))*(1-deadzone)+deadzone, math.cos(rad_angle))

                    x = 32767 * float_x
                    y = -32767 * float_y
                    output_axes = str(int(x)) + ':' + str(int(y))

                    is_axis = False
                    continue

                if single_input == 'F':
                    is_axis = True
                    continue

                if single_input == 'O':
                    output_keys = "ff0d"
                else:
                    output_keys = ''

                # Look at the mapping of the action
                mapped_index = button_mapping.find(single_input)
                output_buttons[mapped_index] = button_order[mapped_index]

            # Write the constructed input line, ignore false positive matches
            output_line = '|' + output_keys + '|' + output_axes + ':0:0:0:0:' + ''.join(output_buttons) + '|\n'
            try:

                for n in range(int(match.group(1))):
                    output_ltm.write(output_line)
            except ValueError:
                print(line)
    print(curLine)
    file.close()

ExportFile(input_file)

output_ltm.close()
