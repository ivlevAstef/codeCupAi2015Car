#pragma once

#ifndef _MY_STRATEGY_H_
#define _MY_STRATEGY_H_

#include "Strategy.h"

#ifdef ENABLE_VISUALIZATOR
#include "Visualizator/Visualizator.h"
#endif

class MyStrategy : public Strategy {
public:
    MyStrategy();

    void move(const model::Car& self, const model::World& world, const model::Game& game, model::Move& move);
private:

#ifdef ENABLE_VISUALIZATOR
    Visualizator visualizator;
#endif
};

#endif
