import random
import datetime

csv_file = r"s:\UnityInstalls\Midterm Polar\Assets\Resources\polarization_tweets.csv"

new_tweets = []
current_id = 20000

categories = [
    {
        "cat": "Progressive Left",
        "type": "polarizing",
        "templates": [
            "We need {kw} now. The {kw2} are destroying society.",
            "If we don't fight for {kw}, the {kw2} will win.",
            "Stand with {kw}. Fight against {kw2}.",
            "We shouldn't accept anything less than {kw}. Down with {kw2}."
        ],
        "kws": ["healthcare", "living wage", "union", "equality", "solidarity", "human rights", "social justice", "climate action", "housing for all"],
        "kws2": ["inequality", "billionaires", "corporate greed", "systemic failure", "Wall Street", "oligarchs"]
    },
    {
        "cat": "Identitarian Left",
        "type": "polarizing",
        "templates": [
            "You must deconstruct your {kw} and challenge {kw2}.",
            "{kw} is a disease. We must end {kw2}.",
            "Stop defending {kw}. It's time to dismantle {kw2}.",
            "If you uphold {kw}, you're complicit in {kw2}."
        ],
        "kws": ["privilege", "whiteness", "heteronormativity", "fragility", "supremacy", "colonization"],
        "kws2": ["patriarchy", "systemic racism", "oppression", "microaggressions", "capitalism"]
    },
    {
        "cat": "Libertarian Right",
        "type": "polarizing",
        "templates": [
            "{kw} is theft. We need absolute {kw2}.",
            "Stop letting the government take your {kw2}. End {kw} now.",
            "{kw} ruins everything. Embrace {kw2}.",
            "Defend your {kw2} from their {kw}."
        ],
        "kws": ["taxation", "regulation", "tyranny", "coercion", "state control", "communism", "fiat currency"],
        "kws2": ["freedom", "free market", "liberty", "property rights", "free speech", "individualism", "sound money"]
    }
]

for cat_data in categories:
    for _ in range(75):
        kw1 = random.choice(cat_data["kws"])
        kw2 = random.choice(cat_data["kws2"])
        text = random.choice(cat_data["templates"]).format(kw=kw1, kw2=kw2)
        kw_col = f"{kw1}, {kw2}"
        
        t = datetime.datetime.now() - datetime.timedelta(days=random.randint(0, 365))
        
        row = [
            f"tw_{current_id:06d}",
            f"user_{random.randint(1000, 9999)}",
            t.strftime("%Y-%m-%d %H:%M:%S"),
            cat_data["cat"],
            cat_data["type"],
            f'"{text}"',
            str(random.randint(10, 10000)),
            str(random.randint(5, 5000)),
            str(random.randint(1, 1000)),
            str(round(random.uniform(-1, 1), 3)),
            str(round(random.uniform(0.5, 1), 3)),
            f'"{kw_col}"'
        ]
        new_tweets.append(",".join(row))
        current_id += 1

with open(csv_file, "a", encoding="utf-8") as f:
    f.write("\n" + "\n".join(new_tweets))

print(f"Added {len(new_tweets)} tweets successfully.")
