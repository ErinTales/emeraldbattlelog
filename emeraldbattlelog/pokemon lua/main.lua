require("tables")
local gBattleMons = 0x02024084
local BATTLE_MON_SIZE = 0x58

console:log("SCRIPT STARTED")

local charmap = {}
local pendingLogs = {}

local charmapPath = "C:/Users/Erin/source/repos/emeraldbattlelog/emeraldbattlelog/pokemon lua/charmap.txt"

for line in io.lines(charmapPath) do
    local character, hex = line:match("^'(.-)'%s*=%s*([0-9A-Fa-f]+)$")

    if character and hex then
        charmap[tonumber(hex, 16)] = character
    end
end
console:log("Loaded " .. tostring(#charmap) .. " entries")

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
	
		--[[console:log(string.format(
		"Battler %d info: %s Lv%d HP %d/%d - %d %d %d %d %d - Moves: %d %s, %d %s, %d %s, %d %s",
		slot,
		speciesName,
		level,
		hp,
		maxHp,
		atk,
		def,
		spa,
		spd,
		spe,
		pp1,
		move1Name,
		pp2,
		move2Name,
		pp3,
		move3Name,
		pp4,
		move4Name
	))
	]]
		pendingLogs[#pendingLogs + 1] = (string.format(
		"Battler %d info: %s Lv%d HP %d/%d - %d %d %d %d %d - Moves: %d %s, %d %s, %d %s, %d %s",
		slot,
		speciesName,
		level,
		hp,
		maxHp,
		atk,
		def,
		spa,
		spd,
		spe,
		pp1,
		move1Name,
		pp2,
		move2Name,
		pp3,
		move3Name,
		pp4,
		move4Name
	))
		--[[pendingLogs[#pendingLogs + 1] = (string.format(
		"Battler %d moves: %d %s, %d %s, %d %s, %d %s",
		slot,
		pp1,
		move1Name,
		pp2,
		move2Name,
		pp3,
		move3Name,
		pp4,
		move4Name
	))]]

end

local lastHP0 = nil
local lastHP1 = nil

local lastPP01 = nil
local lastPP02 = nil
local lastPP03 = nil
local lastPP04 = nil

local lastPP11 = nil
local lastPP12 = nil
local lastPP13 = nil
local lastPP14 = nil

local lastPP21 = nil
local lastPP22 = nil
local lastPP23 = nil
local lastPP24 = nil

local lastPP31 = nil
local lastPP32 = nil
local lastPP33 = nil
local lastPP34 = nil

callbacks:add("frame", function()
    local hp0 = emu:read16(0x020240AC)
    local hp1 = emu:read16(0x02024104)
	local hp2 = emu:read16(0x202415C)
	local hp3 = emu:read16(0x20241B4)

    if hp0 ~= lastHP0 then
        dumpBattler(0)
        lastHP0 = hp0
    end

    if hp1 ~= lastHP1 then
        dumpBattler(1)
        lastHP1 = hp1
    end
	
	if hp2 ~= lastHP2 then
        dumpBattler(2)
        lastHP2 = hp2
    end

    if hp3 ~= lastHP3 then
        dumpBattler(3)
        lastHP3 = hp3
    end
	
	--[[local PP01 = emu:read8(0x020240B4)
	local PP02 = emu:read8(0x020240B5)
	local PP03 = emu:read8(0x020240B6)
	local PP04 = emu:read8(0x020240B7)
	
	if PP01 ~= lastPP01 or PP02 ~= lastPP02
	or PP03 ~= lastPP03 or PP04 ~= lastPP04
	then	
		dumpBattler(0)
		lastPP01 = PP01
		lastPP02 = PP02
		lastPP03 = PP03
		lastPP04 = PP04
	end

	local PP11 = emu:read8(0x0202410C)
	local PP12 = emu:read8(0x0202410D)
	local PP13 = emu:read8(0x0202410E)
	local PP14 = emu:read8(0x0202410F)

	if PP11 ~= lastPP11 or PP12 ~= lastPP12
	or PP13 ~= lastPP13 or PP14 ~= lastPP14
	then	
		dumpBattler(1)
		lastPP11 = PP11
		lastPP12 = PP12
		lastPP13 = PP13
		lastPP14 = PP14
	end

	local PP21 = emu:read8(0x02024164)
	local PP22 = emu:read8(0x02024165)
	local PP23 = emu:read8(0x02024166)
	local PP24 = emu:read8(0x02024167)
	
	if PP21 ~= lastPP21 or PP22 ~= lastPP22
	or PP23 ~= lastPP23 or PP24 ~= lastPP24
	then	
		dumpBattler(2)
		lastPP21 = PP21
		lastPP22 = PP22
		lastPP23 = PP23
		lastPP24 = PP24
	end

	local PP31 = emu:read8(0x020241BC)
	local PP32 = emu:read8(0x020241BD)
	local PP33 = emu:read8(0x020241BE)
	local PP34 = emu:read8(0x020241BF)
	
	if PP31 ~= lastPP31 or PP32 ~= lastPP32
	or PP33 ~= lastPP33 or PP34 ~= lastPP34		
	then
		dumpBattler(3)
		lastPP31 = PP31
		lastPP32 = PP32
		lastPP33 = PP33
		lastPP34 = PP34
	end--]]
end)

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

local frameCounter = 0

callbacks:add("frame", function()
    frameCounter = frameCounter + 1

    if frameCounter % 60 == 0 and #pendingLogs > 0 then
	
		dumpBattler(0)
		dumpBattler(1)
		dumpBattler(2)
		dumpBattler(3)
		
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
