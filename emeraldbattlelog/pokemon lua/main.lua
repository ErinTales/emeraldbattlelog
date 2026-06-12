require("tables")
local gBattleMons = 0x02024084
local BATTLE_MON_SIZE = 0x58

console:log("SCRIPT STARTED")

local charmap = {}
local pendingLogs = {}

lastBattlers = {
    {},
    {},
    {},
    {}
}

local charmapPath = "C:/Users/Erin/source/repos/emeraldbattlelog/emeraldbattlelog/pokemon lua/charmap.txt"

for line in io.lines(charmapPath) do
    local character, hex = line:match("^'(.-)'%s*=%s*([0-9A-Fa-f]+)$")

    if character and hex then
        charmap[tonumber(hex, 16)] = character
    end
end
console:log("Loaded " .. tostring(#charmap) .. " entries")

--Who the fuck knows what this does.
local function decodePokemonString(addr, maxLen)
    local out = {}

    for i = 0, maxLen - 1 do
        local b = emu:read8(addr + i)

        if b == 0xFF then
            break
        end

        out[#out + 1] = charmap[b] or string.format("<%02X>", b)
    end

    return table.concat(out)
end

--Dump battler info to file.
local function dumpBattler(slot)
    local base = gBattleMons + slot * BATTLE_MON_SIZE

    local speciesId = emu:read16(base + 0x00)
	local speciesName = pokemontbl[speciesId]

    local move1Id = emu:read16(base + 0x0C)
	local move1Name = movetbl[move1Id]
    local move2Id = emu:read16(base + 0x0E)
	local move2Name = movetbl[move2Id]
    local move3Id = emu:read16(base + 0x10)
	local move3Name = movetbl[move3Id]
    local move4Id = emu:read16(base + 0x12)
	local move4Name = movetbl[move4Id]
	
	local pp1 = emu:read8(base + 0x24)
	local pp2 = emu:read8(base + 0x25)
	local pp3 = emu:read8(base + 0x26)
	local pp4 = emu:read8(base + 0x27)

    local hp = emu:read16(base + 0x28)
    local level = emu:read8(base + 0x2A)
    local maxHp = emu:read16(base + 0x2C)
	
	local atk = emu:read16(base + 0x02)
	local def = emu:read16(base + 0x04)
	local spe = emu:read16(base + 0x06)
	local spa = emu:read16(base + 0x08)
	local spd = emu:read16(base + 0x0A)
	
	--leaving this but commenting it out, it prints to the console for debugging purposes.
		--[[console:log(string.format(
		"Battler %d info: %s Lv%d HP %d/%d - %d %d %d %d %d - Moves: %d %s, %d %s, %d %s, %d %s", slot, speciesName, level, hp, maxHp, atk, def, spa, spd, spe, pp1, move1Name, pp2, move2Name, pp3, move3Name, pp4, move4Name
	))
	]]
		pendingLogs[#pendingLogs + 1] = (string.format(
		"Battler %d info: %s Lv%d HP %d/%d - %d %d %d %d %d - Moves: %d %s, %d %s, %d %s, %d %s", slot, speciesName, level, hp, maxHp, atk, def, spa, spd, spe, pp1, move1Name, pp2, move2Name, pp3, move3Name, pp4, move4Name
	))

end

--Checks if battlers have changed. (This triggers more frequently than necessary, but making it better would involve actually knowing what is happening.
function checkBattler(index, base)
    local changed = false

    for i = 0, 0x57 do
        local value = emu:read8(base + i)

        if lastBattlers[index + 1][i] ~= value then
            lastBattlers[index + 1][i] = value
            changed = true
        end
    end

    if changed then
        dumpBattler(index)
    end
end

--Two functions to capture buffer text.
local DISPLAYED_STRING_BATTLE = 0x02022E2C

local lastString = ""

callbacks:add("frame", function()
    local str = decodePokemonString(DISPLAYED_STRING_BATTLE, 300)

    if str ~= "" and str ~= lastString then
        console:log(str)
        lastString = str
    end
end)

local lastString2 = ""

callbacks:add("frame", function()
    local str = decodePokemonString(DISPLAYED_STRING_BATTLE, 300)
    if str ~= "" and str ~= lastString2 then
        pendingLogs[#pendingLogs + 1] = str
        lastString2 = str
    end
end)

--Check if battlers have changed
callbacks:add("frame", function()
    checkBattler(0, 0x02024090)
    checkBattler(1, 0x020240E8)
    checkBattler(2, 0x02024140)
    checkBattler(3, 0x02024198)
end)

local frameCounter = 0
--Print logs to file
callbacks:add("frame", function()
    frameCounter = frameCounter + 1

    if frameCounter % 60 == 0 and #pendingLogs > 0 then
        local logfile = io.open(
            "C:/Users/Erin/source/repos/emeraldbattlelog/emeraldbattlelog/pokemon lua/battlelog.txt",
            "a"
        )

        if logfile then
            for _, line in ipairs(pendingLogs) do
                logfile:write(line .. "\n")
            end

            logfile:close()
        end

        pendingLogs = {}
    end
end)
