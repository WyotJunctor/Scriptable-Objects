from os import listdir
from os.path import isfile, join
from PIL import Image, ImageChops, ImageOps
import math

def trim(im):
    if im.load()[0,0] != (0,0,0):
        return im
    bg = Image.new(im.mode, im.size, im.getpixel((0,0)))
    diff = ImageChops.difference(im, bg)
    diff = ImageChops.add(diff, diff, .5, -100)
    bbox = diff.getbbox()
    if bbox:
        print(bbox)
        return im.crop(bbox)

class BG:
    img_size = None
    rows = 0
    cols = 0
    count = 0
    full = 0
    params = None
    bg = None
    sheet_count = 0

def initialize(img_names, flip_wide):
    im = Image.open(img_names[0])
    im = ImageOps.expand(im, border=10)
    if im.size[0] > im.size[1] and flip_wide:
        im = im.rotate(-90, expand=True)
    im = trim(im)
    im.save(img_names[0])
    BG.img_size = Image.open(img_names[0]).size
    BG.rows = 7
    BG.cols = 10
    BG.count = 0
    BG.full = BG.rows*BG.cols
    BG.params = ("RGBA", (BG.cols*BG.img_size[0], BG.rows*BG.img_size[1]), (0,0,0,0))
    BG.bg = Image.new(*BG.params)
    BG.sheet_count = 0

def do_that_shit(
        card_type,
        group,
        flip_wide=True,
        generate_sheet=True,
    ):
    path = f"{card_type}/{group}/"
    img_names = [f"{path}{f}" for f in listdir(path)
             if isfile(join(path, f))
             and ".png" in join(path, f)
             and "sheet" not in join(path, f)]
    initialize(img_names, flip_wide)
    for img_name in img_names:
        print(img_name)
        im = Image.open(img_name)
        im = ImageOps.expand(im, border=10)
        if im.size[0] > im.size[1] and flip_wide:
            im = im.rotate(-90, expand=True)
        im = trim(im)
        im.save(img_name)
        if not generate_sheet:
            continue
        column = BG.count % BG.cols
        row = math.floor(BG.count / BG.cols)
        offset = (column*BG.img_size[0], row*BG.img_size[1])
        BG.bg.paste(im, offset)
        BG.count += 1
        if BG.count == BG.full:
            BG.bg.show()
            BG.bg.save(f"{path}sheet_{card_type}_{group}_{BG.sheet_count}")
            BG.sheet_count += 1
            BG.count = 0
            BG.bg = Image.new(*BG.params)

    if (generate_sheet):
        BG.bg.show()
        BG.bg.save(f"{path}sheet_{card_type}_{group}_{BG.sheet_count}.png")



architects = ["THRALLTAKER", "LIMBSNATCHER", "WILLBREAKER", "PLANEWARPER"]
factions = ["COUNCIL", "GUILD", "ORDER", "BARONY"]
decks = {
    "ACTION": architects,
    #"ASSET": architects,
    #"ROOM": architects,
    #"STRATEGY": factions,
    #"BACK": architects + factions,
}
tokens = {
    "FRONT_HERO": factions,
    "BACK_HERO": factions,
}

for deck, groups in decks.items():
    for group in groups:
        do_that_shit(card_type=deck, group=group)

"""
for token, groups in tokens.items():
    for group in groups:
        do_that_shit(card_type=token, group=group, flip_wide=False, generate_sheet=False)
"""
#do_that_shit(card_type="ROOM", group="LIMBSNATCHER")
